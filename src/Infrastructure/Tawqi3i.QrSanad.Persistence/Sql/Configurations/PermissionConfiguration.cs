namespace Tawqi3i.QrSanad.Persistence.Sql.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Persistence.Sql.Seeding;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Code).HasMaxLength(128).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(256).IsRequired();

        builder.HasIndex(a => a.Code).IsUnique();

        builder.HasData(SeedsFactory.GetPermissions());
    }
}
