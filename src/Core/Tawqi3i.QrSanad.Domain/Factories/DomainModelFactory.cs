namespace Tawqi3i.QrSanad.Domain.Factories;

using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Auth;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Branches;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Devices;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Users;
using Tawqi3i.QrSanad.Common.Enums;
using Tawqi3i.QrSanad.Common.Helpers;
using Tawqi3i.QrSanad.Common.Security;
using Tawqi3i.QrSanad.Common.ViewModels.V1;

internal static class DomainModelFactory
{
    public static User CreateAdmin(UserCreateRequest request, Guid branchId, string passwordHash, IEnumerable<Permission> permissions, Guid actorId, DateTime now)
    {
        var admin = CreateAccount(UserType.Admin, request.UserName, request.DisplayName, branchId, passwordHash, actorId, now);
        var email = request.Email.Trim();

        admin.Email = email;
        admin.NormalizedEmail = Normalizer.Normalize(email);
        admin.PhoneNumber = NullIfBlank(request.PhoneNumber);
        admin.MustChangePassword = request.MustChangePassword;
        admin.UserPermissions = permissions
            .Select(a => new UserPermission { UserId = admin.Id, PermissionId = a.Id, AssignedAt = now, AssignedBy = actorId })
            .ToList();

        return admin;
    }

    public static User CreateDevice(DeviceCreateRequest request, Guid branchId, string passwordHash, Guid actorId, DateTime now)
    {
        return CreateAccount(UserType.Device, request.UserName, request.DisplayName, branchId, passwordHash, actorId, now);
    }

    public static Branch CreateBranch(BranchRequest request, Guid actorId)
    {
        var name = request.Name.Trim();

        return new Branch
        {
            Id = Generator.CreateV7Guid(),
            Name = name,
            NormalizedName = Normalizer.Normalize(name),
            Code = NullIfBlank(request.Code),
            Address = NullIfBlank(request.Address),
            PhoneNumber = NullIfBlank(request.PhoneNumber),
            Status = BranchStatus.Active,
            ConcurrencyStamp = Generator.NewStamp(),
            CreatedBy = actorId
        };
    }

    public static PasswordPolicySettings CreatePasswordPolicy(PasswordPolicyRequest request)
    {
        return new PasswordPolicySettings
        {
            MinLength = request.MinLength,
            MaxLength = request.MaxLength,
            RequireUppercase = request.RequireUppercase,
            RequireLowercase = request.RequireLowercase,
            RequireDigit = request.RequireDigit,
            RequireNonAlphanumeric = request.RequireNonAlphanumeric
        };
    }

    public static IdentityCard CreateIdentityCard(IdentityCardViewModel card)
    {
        return new IdentityCard
        {
            NationalNumber = card.NationalNumber,
            ArabicName = card.ArabicName,
            EnglishName = card.EnglishName,
            Gender = card.Gender,
            BloodType = card.BloodType,
            BirthDate = card.BirthDate,
            MotherName = card.MotherName,
            RegistryNumber = card.RegistryNumber,
            ExpiryDate = card.ExpiryDate,
            Residence = card.Residence,
            CardNumber = card.CardNumber
        };
    }

    public static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static User CreateAccount(UserType type, string userName, string displayName, Guid branchId, string passwordHash, Guid actorId, DateTime now)
    {
        userName = userName.Trim();

        return new User
        {
            Id = Generator.CreateV7Guid(),
            UserName = userName,
            NormalizedUserName = Normalizer.Normalize(userName),
            DisplayName = displayName.Trim(),
            UserType = type,
            BranchId = branchId,
            PasswordHash = passwordHash,
            SecurityStamp = Generator.NewStamp(),
            ConcurrencyStamp = Generator.NewStamp(),
            Status = UserStatus.Active,
            PasswordChangedAt = now,
            CreatedBy = actorId
        };
    }
}
