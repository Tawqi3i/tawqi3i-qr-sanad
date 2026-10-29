namespace Tawqi3i.QrSanad.DomainTests;

using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Users;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Enums;
using Tawqi3i.QrSanad.Common.ViewModels.V1;
using Tawqi3i.QrSanad.Domain.Services.V1.Users;

public class UsersServiceTests
{
    private ServiceTestHost host = null!;
    private Guid branchA;
    private Guid branchB;

    [SetUp]
    public async Task SetUp()
    {
        this.host = await ServiceTestHost.Create(ServiceTestHost.LicenseWith(maxUsers: 1));
        this.branchA = await this.host.AddBranch();
        this.branchB = await this.host.AddBranch();
    }

    [TearDown]
    public async Task TearDown() => await this.host.DisposeAsync();

    [Test]
    public async Task Create_GivesANewAdminEveryPermission()
    {
        var response = await this.host.CreateAdmin(this.branchA);

        Assert.That(response.StatusCode, Is.EqualTo(201), response.Message);
        Assert.That(response.Data!.Type, Is.EqualTo(UserType.Admin));
        Assert.That(response.Data.BranchId, Is.EqualTo(this.branchA));
        Assert.That(response.Data.Permissions, Is.EquivalentTo(PermissionCodes.AllCodes));
    }

    [Test]
    public async Task Create_OneAdminPerBranch()
    {
        await this.host.AddAdmin(this.branchA);

        var response = await this.host.CreateAdmin(this.branchA);

        Assert.That(response.StatusCode, Is.EqualTo(409));
        Assert.That(response.Message, Is.EqualTo(ResponseMessages.BranchHasAdmin));
    }

    [Test]
    public async Task Create_StopsAtTheLicenseUserLimit()
    {
        await this.host.AddAdmin(this.branchA);

        var response = await this.host.CreateAdmin(this.branchB);

        Assert.That(response.StatusCode, Is.EqualTo(403));
        Assert.That(response.Message, Is.EqualTo(ResponseMessages.LicenseUserLimitReached));
    }

    [Test]
    public async Task Create_DevicesDoNotCountAsUsers()
    {
        await this.host.AddDevice(this.branchA);
        await this.host.AddDevice(this.branchA);

        var response = await this.host.CreateAdmin(this.branchA);

        Assert.That(response.StatusCode, Is.EqualTo(201), response.Message);
    }

    [Test]
    public async Task Delete_FreesTheBranchForANewAdmin()
    {
        var admin = await this.host.AddAdmin(this.branchA);

        var deleted = await this.host.Call<IUsersService, Response>(a => a.Delete(admin, this.host.SuperAdmin));
        var replacement = await this.host.CreateAdmin(this.branchA);

        Assert.That(deleted.StatusCode, Is.EqualTo(200), deleted.Message);
        Assert.That(replacement.StatusCode, Is.EqualTo(201), replacement.Message);
    }

    [Test]
    public async Task Users_DoNotIncludeDevices()
    {
        var admin = await this.host.AddAdmin(this.branchA);
        var device = await this.host.AddDevice(this.branchA);

        var page = await this.host.Call<IUsersService, PagedResponse<UserViewModel>>(a => a.GetAll(new UsersQuery()));
        var get = await this.host.Call<IUsersService, Response<UserDetailsViewModel>>(a => a.Get(device));

        Assert.That(page.Items.Select(a => a.Id), Is.EquivalentTo(new[] { this.host.SuperAdmin.Id, admin }));
        Assert.That(get.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task SuperAdmin_CannotDisableItself()
    {
        var response = await this.host.Call<IUsersService, Response>(a =>
            a.SetStatus(this.host.SuperAdmin.Id, new UserStatusRequest { Status = UserStatus.Disabled }, this.host.SuperAdmin));

        Assert.That(response.StatusCode, Is.EqualTo(400));
        Assert.That(response.Message, Is.EqualTo(ResponseMessages.CannotModifySelf));
    }

    [Test]
    public async Task SetPermissions_NarrowsTheAdmin()
    {
        var admin = await this.host.AddAdmin(this.branchA);
        var request = new PermissionsRequest { Permissions = [PermissionCodes.Devices.View] };

        var response = await this.host.Call<IUsersService, Response<UserDetailsViewModel>>(a => a.SetPermissions(admin, request, this.host.SuperAdmin));

        Assert.That(response.StatusCode, Is.EqualTo(200), response.Message);
        Assert.That(response.Data!.Permissions, Is.EquivalentTo(new[] { PermissionCodes.Devices.View }));
    }
}
