namespace Tawqi3i.QrSanad.DomainTests;

using Microsoft.Extensions.DependencyInjection;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Users;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Enums;
using Tawqi3i.QrSanad.Common.Licensing;
using Tawqi3i.QrSanad.Common.Persistence;
using Tawqi3i.QrSanad.Common.ViewModels.V1;
using Tawqi3i.QrSanad.Domain.Factories;
using Tawqi3i.QrSanad.Domain.Services.V1.Devices;

public class DevicesServiceTests
{
    private ServiceTestHost host = null!;
    private Guid branchA;
    private Guid branchB;
    private Guid adminA;

    [SetUp]
    public async Task SetUp()
    {
        this.host = await ServiceTestHost.Create(ServiceTestHost.LicenseWith(maxDevices: 3));
        this.branchA = await this.host.AddBranch();
        this.branchB = await this.host.AddBranch();
        this.adminA = await this.host.AddAdmin(this.branchA);
    }

    [TearDown]
    public async Task TearDown() => await this.host.DisposeAsync();

    [Test]
    public async Task Create_SuperAdminMustNameTheBranch()
    {
        var response = await this.host.CreateDevice(this.host.SuperAdmin, branchId: null);

        Assert.That(response.StatusCode, Is.EqualTo(400));
        Assert.That(response.Message, Is.EqualTo(ResponseMessages.BranchRequired));
    }

    [Test]
    public async Task Create_AdminAddsToOwnBranch()
    {
        var response = await this.host.CreateDevice(ServiceTestHost.As(this.adminA), branchId: null);

        Assert.That(response.StatusCode, Is.EqualTo(201), response.Message);
        Assert.That(response.Data!.Type, Is.EqualTo(UserType.Device));
        Assert.That(response.Data.BranchId, Is.EqualTo(this.branchA));
        Assert.That(response.Data.Email, Is.Null);
        Assert.That(response.Data.Permissions, Is.Empty);
    }

    [Test]
    public async Task Create_AdminCannotAddToAnotherBranch()
    {
        var response = await this.host.CreateDevice(ServiceTestHost.As(this.adminA), this.branchB);

        Assert.That(response.StatusCode, Is.EqualTo(404));
        Assert.That(response.Message, Is.EqualTo(ResponseMessages.BranchNotFound));
    }

    [Test]
    public async Task Create_StopsAtTheLicenseDeviceLimit()
    {
        await this.host.AddDevice(this.branchA);
        await this.host.AddDevice(this.branchA);
        await this.host.AddDevice(this.branchB);

        var response = await this.host.CreateDevice(this.host.SuperAdmin, this.branchB);

        Assert.That(response.StatusCode, Is.EqualTo(403));
        Assert.That(response.Message, Is.EqualTo(ResponseMessages.LicenseDeviceLimitReached));
    }

    [Test]
    public async Task Delete_FreesTheLicensePlace()
    {
        var device = await this.host.AddDevice(this.branchA);
        await this.host.AddDevice(this.branchA);
        await this.host.AddDevice(this.branchA);

        var deleted = await this.host.Call<IDevicesService, Response>(a => a.Delete(device, this.host.SuperAdmin));
        var created = await this.host.CreateDevice(this.host.SuperAdmin, this.branchA);

        Assert.That(deleted.StatusCode, Is.EqualTo(200), deleted.Message);
        Assert.That(created.StatusCode, Is.EqualTo(201), created.Message);
    }

    [Test]
    public async Task Admin_ReachesOnlyOwnBranchDevices()
    {
        var own = await this.host.AddDevice(this.branchA);
        var other = await this.host.AddDevice(this.branchB);
        var admin = ServiceTestHost.As(this.adminA);

        var page = await this.host.Call<IDevicesService, PagedResponse<UserViewModel>>(a => a.GetAll(new UsersQuery(), admin));
        var get = await this.host.Call<IDevicesService, Response<UserDetailsViewModel>>(a => a.Get(other, admin));
        var delete = await this.host.Call<IDevicesService, Response>(a => a.Delete(other, admin));

        Assert.That(page.Items.Select(a => a.Id), Is.EquivalentTo(new[] { own }));
        Assert.That(get.StatusCode, Is.EqualTo(404));
        Assert.That(get.Message, Is.EqualTo(ResponseMessages.DeviceNotFound));
        Assert.That(delete.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task Admin_CannotReachAdminsThroughDevices()
    {
        var otherAdmin = await this.host.AddAdmin(this.branchB);

        var response = await this.host.Call<IDevicesService, Response>(a => a.Delete(otherAdmin, this.host.SuperAdmin));

        Assert.That(response.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task Disable_EndsTheDeviceSessions()
    {
        var device = await this.host.AddDevice(this.branchA);
        var stampBefore = await this.SecurityStampOf(device);

        var response = await this.host.Call<IDevicesService, Response>(a =>
            a.SetStatus(device, new UserStatusRequest { Status = UserStatus.Disabled }, ServiceTestHost.As(this.adminA)));

        Assert.That(response.StatusCode, Is.EqualTo(200), response.Message);
        Assert.That(await this.SecurityStampOf(device), Is.Not.EqualTo(stampBefore));
    }

    [Test]
    public async Task ConcurrentAdds_CannotBothTakeTheLastPlace()
    {
        await this.host.AddDevice(this.branchA);
        await this.host.AddDevice(this.branchA);

        // Two requests check the limit before either saves: both see one place left.
        await using var first = this.host.CreateScope();
        await using var second = this.host.CreateScope();

        Assert.That(await LicenseCheck(first), Is.Null);
        Assert.That(await LicenseCheck(second), Is.Null);

        this.StageDevice(first);
        this.StageDevice(second);

        await first.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChanges();

        var conflict = Assert.ThrowsAsync<PersistenceException>(() => second.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChanges());
        Assert.That(conflict!.Code, Is.EqualTo(ErrorCodes.ConcurrencyConflict));
    }

    private static Task<Response?> LicenseCheck(AsyncServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<ILicenseService>().EnsureCanAddDevice();

    private void StageDevice(AsyncServiceScope scope)
    {
        var device = DomainModelFactory.CreateDevice(this.host.NewDevice(), this.branchA, "hash", this.host.SuperAdmin.Id, DateTime.UtcNow);
        scope.ServiceProvider.GetRequiredService<IUsersRepository>().AddOnly(device);
    }

    private Task<string> SecurityStampOf(Guid id) =>
        this.host.Call<IUsersRepository, string>(a => Task.FromResult(a.GetQueryable().Single(u => u.Id == id).SecurityStamp));
}
