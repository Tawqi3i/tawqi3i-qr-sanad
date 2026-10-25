namespace Tawqi3i.QrSanad.Persistence.Sql.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;

internal sealed class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.HasKey(a => new { a.UserId, a.PermissionId });

        builder.HasOne(a => a.User)
            .WithMany(a => a.UserPermissions)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Permission)
            .WithMany(a => a.UserPermissions)
            .HasForeignKey(a => a.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.PermissionId);
    }
}
