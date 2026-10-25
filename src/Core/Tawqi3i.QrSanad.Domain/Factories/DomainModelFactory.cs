namespace Tawqi3i.QrSanad.Domain.Factories;

using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Auth;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Branches;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Users;
using Tawqi3i.QrSanad.Common.Enums;
using Tawqi3i.QrSanad.Common.Helpers;
using Tawqi3i.QrSanad.Common.Security;

internal static class DomainModelFactory
{
    public static User CreateUser(UserCreateRequest request, string passwordHash, Guid branchId, IEnumerable<Permission> permissions, Guid actorId, DateTime now)
    {
        var userId = Generator.CreateV7Guid();
        var userName = request.UserName.Trim();
        var email = request.Email.Trim();

        return new User
        {
            Id = userId,
            UserName = userName,
            NormalizedUserName = Normalizer.Normalize(userName),
            Email = email,
            NormalizedEmail = Normalizer.Normalize(email),
            DisplayName = request.DisplayName.Trim(),
            PhoneNumber = NullIfBlank(request.PhoneNumber),
            UserType = request.Type,
            BranchId = branchId,
            PasswordHash = passwordHash,
            SecurityStamp = Generator.NewStamp(),
            ConcurrencyStamp = Generator.NewStamp(),
            Status = UserStatus.Active,
            MustChangePassword = request.MustChangePassword,
            PasswordChangedAt = now,
            CreatedBy = actorId,
            UserPermissions = permissions
                .Select(a => new UserPermission { UserId = userId, PermissionId = a.Id, AssignedAt = now, AssignedBy = actorId })
                .ToList()
        };
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

    public static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
