namespace Tawqi3i.QrSanad.DomainTests;

using System.Security.Cryptography;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Branches;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Devices;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Users;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Enums;
using Tawqi3i.QrSanad.Common.Helpers;
using Tawqi3i.QrSanad.Common.Identity;
using Tawqi3i.QrSanad.Common.Licensing;
using Tawqi3i.QrSanad.Common.Security;
using Tawqi3i.QrSanad.Common.ViewModels.V1;
using Tawqi3i.QrSanad.Domain;
using Tawqi3i.QrSanad.Domain.Services.V1.Branches;
using Tawqi3i.QrSanad.Domain.Services.V1.Devices;
using Tawqi3i.QrSanad.Domain.Services.V1.Users;
using Tawqi3i.QrSanad.Identity;
using Tawqi3i.QrSanad.Licensing;
using Tawqi3i.QrSanad.Persistence;
using Tawqi3i.QrSanad.Persistence.Sql;
using Tawqi3i.QrSanad.Persistence.Sql.Providers;

/// <summary>
/// The real domain services, repositories and license checks over an in-memory SQLite database,
/// with a SuperAdmin and a license signed by a key made for the test. Each call runs in its own
/// scope, like a request.
/// </summary>
internal sealed class ServiceTestHost : IAsyncDisposable
{
    public const string Password = "Test#Pass12345";

    private readonly SqliteConnection connection;
    private readonly ServiceProvider provider;
    private int sequence;

    private ServiceTestHost(SqliteConnection connection, ServiceProvider provider, RequestUser superAdmin)
    {
        this.connection = connection;
        this.provider = provider;
        this.SuperAdmin = superAdmin;
    }

    public RequestUser SuperAdmin { get; }

    public static async Task<ServiceTestHost> Create(LicenseData license)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        using var signingKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var services = new ServiceCollection();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(new PasswordPolicySettings());
        services.AddSingleton(new AuthSettings());
        services.AddDistributedMemoryCache();
        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddScoped<IUserAccessProvider, UserAccessProvider>();
        services.AddScoped<ISecurityAuditService, SecurityAuditService>();
        services.AddSingleton(new LicensingSettings());
        services.AddSingleton(new LicenseVerifier(signingKey.ExportSubjectPublicKeyInfo()));
        services.AddScoped<ILicenseService, LicenseService>();
        services.AddDbContext<SqlServerQrSanadContext>(options => options.UseSqlite(connection));
        services.AddScoped<QrSanadSqlContext>(provider => provider.GetRequiredService<SqlServerQrSanadContext>());
        services.AddRepositories();
        services.AddDomainServices(new ConfigurationBuilder().Build());

        var provider = services.BuildServiceProvider();

        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<QrSanadSqlContext>();
        await context.Database.EnsureCreatedAsync();

        var payload = LicenseKeyFormat.Serialize(license);
        context.Licenses.Add(new License
        {
            Id = License.SingletonId,
            Key = LicenseKeyFormat.Encode(payload, signingKey.SignData(payload, HashAlgorithmName.SHA256)),
            ConcurrencyStamp = Generator.NewStamp()
        });

        var superAdmin = new User
        {
            Id = Generator.CreateV7Guid(),
            UserName = "superadmin",
            NormalizedUserName = Normalizer.Normalize("superadmin"),
            Email = "superadmin@test.local",
            NormalizedEmail = Normalizer.Normalize("superadmin@test.local"),
            DisplayName = "Super Admin",
            UserType = UserType.SuperAdmin,
            PasswordHash = scope.ServiceProvider.GetRequiredService<IPasswordService>().Hash(Password),
            SecurityStamp = Generator.NewStamp(),
            ConcurrencyStamp = Generator.NewStamp(),
            Status = UserStatus.Active
        };

        context.Users.Add(superAdmin);
        await context.SaveChangesAsync();

        return new ServiceTestHost(connection, provider, new RequestUser { Id = superAdmin.Id });
    }

    public static LicenseData LicenseWith(int? maxUsers = null, int? maxDevices = null, int? maxBranches = null) => new()
    {
        Customer = "Test Customer",
        Type = "DEV",
        IssuedAt = DateTime.UtcNow.AddDays(-1),
        ExpiresAt = DateTime.UtcNow.AddDays(30),
        MaxUsers = maxUsers,
        MaxDevices = maxDevices,
        MaxBranches = maxBranches
    };

    public static RequestUser As(Guid id) => new() { Id = id };

    public async Task<T> Call<TService, T>(Func<TService, Task<T>> call)
        where TService : notnull
    {
        await using var scope = this.provider.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<TService>());
    }

    public AsyncServiceScope CreateScope() => this.provider.CreateAsyncScope();

    public async Task<Guid> AddBranch()
    {
        var request = new BranchRequest { Name = $"Branch {this.Next()}" };
        var created = await this.Call<IBranchesService, Response<BranchViewModel>>(a => a.Create(request, this.SuperAdmin));

        return Created(created).Id;
    }

    public Task<Response<UserDetailsViewModel>> CreateAdmin(Guid branchId)
    {
        var userName = $"admin{this.Next()}";
        var request = new UserCreateRequest
        {
            UserName = userName,
            Email = $"{userName}@test.local",
            DisplayName = "Branch Admin",
            Password = Password,
            BranchId = branchId
        };

        return this.Call<IUsersService, Response<UserDetailsViewModel>>(a => a.Create(request, this.SuperAdmin));
    }

    public async Task<Guid> AddAdmin(Guid branchId) => Created(await this.CreateAdmin(branchId)).Id;

    public Task<Response<UserDetailsViewModel>> CreateDevice(RequestUser actor, Guid? branchId) =>
        this.Call<IDevicesService, Response<UserDetailsViewModel>>(a => a.Create(this.NewDevice(branchId), actor));

    public async Task<Guid> AddDevice(Guid branchId) => Created(await this.CreateDevice(this.SuperAdmin, branchId)).Id;

    public DeviceCreateRequest NewDevice(Guid? branchId = null) => new()
    {
        UserName = $"device{this.Next()}",
        DisplayName = "Gate",
        Password = Password,
        BranchId = branchId
    };

    public async ValueTask DisposeAsync()
    {
        await this.provider.DisposeAsync();
        await this.connection.DisposeAsync();
    }

    private static T Created<T>(Response<T> response)
    {
        Assert.That(response.StatusCode, Is.EqualTo(201), response.Message);
        return response.Data!;
    }

    private int Next() => Interlocked.Increment(ref this.sequence);
}
