namespace Tawqi3i.QrSanad.Domain;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tawqi3i.QrSanad.Domain.Services.V1;
using Tawqi3i.QrSanad.Domain.Services.V1.Audit;
using Tawqi3i.QrSanad.Domain.Services.V1.Branches;
using Tawqi3i.QrSanad.Domain.Services.V1.Devices;
using Tawqi3i.QrSanad.Domain.Services.V1.PasswordPolicies;
using Tawqi3i.QrSanad.Domain.Services.V1.Permissions;
using Tawqi3i.QrSanad.Domain.Services.V1.QRSanad;
using Tawqi3i.QrSanad.Domain.Services.V1.Users;

public static class Registration
{
    public static void AddDomainServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AccessGuard>();
        services.AddScoped<AccountOperations>();
        services.AddScoped<IBranchesService, BranchesService>();
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<IDevicesService, DevicesService>();
        services.AddScoped<IPermissionsService, PermissionsService>();
        services.AddScoped<ISecurityAuditLogsService, SecurityAuditLogsService>();
        services.AddScoped<IQRSanadService, QRSanadService>();
        services.AddScoped<IScanStatsService, ScanStatsService>();
        services.AddScoped<IPasswordPolicyService, PasswordPolicyService>();
    }
}
