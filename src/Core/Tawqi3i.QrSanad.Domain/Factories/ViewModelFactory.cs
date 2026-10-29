namespace Tawqi3i.QrSanad.Domain.Factories;

using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Identity;
using Tawqi3i.QrSanad.Common.Security;
using Tawqi3i.QrSanad.Common.ViewModels.V1;

internal static class ViewModelFactory
{
    public static PagedResponse<UserViewModel> CreatePaginated(ICollection<User> items, int totalCount, PagedQuery query, DateTime now)
    {
        return PagedResponse<UserViewModel>.Ok(items.Select(a => Fill(new UserViewModel(), a, now)).ToList(), totalCount, query);
    }

    /// <summary>
    /// Expects the branch and permissions to be loaded.
    /// </summary>
    public static UserDetailsViewModel CreateDetails(User user, UserAccess? access, DateTime now)
    {
        var model = Fill(new UserDetailsViewModel(), user, now);

        model.Permissions = access?.Permissions
            ?? user.UserPermissions.Select(a => a.Permission.Code).Order(StringComparer.Ordinal).ToArray();

        return model;
    }

    public static BranchViewModel Create(Branch branch, User? admin, int devicesCount)
    {
        return new BranchViewModel
        {
            Id = branch.Id,
            Name = branch.Name,
            Code = branch.Code,
            Address = branch.Address,
            PhoneNumber = branch.PhoneNumber,
            Status = branch.Status,
            Admin = admin == null
                ? null
                : new UserSummaryViewModel
                {
                    Id = admin.Id,
                    UserName = admin.UserName,
                    DisplayName = admin.DisplayName,
                    Status = admin.Status
                },
            DevicesCount = devicesCount,
            CreatedAt = branch.CreatedAt,
            UpdatedAt = branch.UpdatedAt
        };
    }

    public static PermissionViewModel Create(Permission permission)
    {
        return new PermissionViewModel
        {
            Id = permission.Id,
            Code = permission.Code,
            Description = permission.Description
        };
    }

    public static PasswordPolicyViewModel Create(PasswordPolicySettings policy)
    {
        return new PasswordPolicyViewModel
        {
            MinLength = policy.MinLength,
            MaxLength = policy.MaxLength,
            RequireUppercase = policy.RequireUppercase,
            RequireLowercase = policy.RequireLowercase,
            RequireDigit = policy.RequireDigit,
            RequireNonAlphanumeric = policy.RequireNonAlphanumeric
        };
    }

    public static SecurityAuditLogViewModel Create(SecurityAuditLog log)
    {
        return new SecurityAuditLogViewModel
        {
            Id = log.Id,
            EventType = log.EventType,
            UserId = log.UserId,
            UserName = log.UserName,
            ActorId = log.ActorId,
            BranchId = log.BranchId,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            Details = log.Details,
            IdentityCard = log.IdentityCard is { } card ? Create(card) : null,
            CreatedAt = log.CreatedAt
        };
    }

    public static IdentityCardViewModel Create(IdentityCard card)
    {
        return new IdentityCardViewModel
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

    private static T Fill<T>(T model, User user, DateTime now)
        where T : UserViewModel
    {
        model.Id = user.Id;
        model.UserName = user.UserName;
        model.Email = user.Email;
        model.DisplayName = user.DisplayName;
        model.PhoneNumber = user.PhoneNumber;
        model.Type = user.UserType;
        model.BranchId = user.BranchId;
        model.BranchName = user.Branch?.Name;
        model.Status = user.Status;
        model.IsLocked = user.LockoutEnd > now;
        model.LockoutEnd = user.LockoutEnd > now ? user.LockoutEnd : null;
        model.MustChangePassword = user.MustChangePassword;
        model.LastLoginAt = user.LastLoginAt;
        model.CreatedAt = user.CreatedAt;
        model.UpdatedAt = user.UpdatedAt;

        return model;
    }
}
