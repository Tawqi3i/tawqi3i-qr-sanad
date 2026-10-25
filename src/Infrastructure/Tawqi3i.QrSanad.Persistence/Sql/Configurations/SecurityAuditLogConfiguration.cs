namespace Tawqi3i.QrSanad.Persistence.Sql.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;

internal sealed class SecurityAuditLogConfiguration : IEntityTypeConfiguration<SecurityAuditLog>
{
    public void Configure(EntityTypeBuilder<SecurityAuditLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.EventType).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(a => a.UserName).HasMaxLength(256);
        builder.Property(a => a.IpAddress).HasMaxLength(64);
        builder.Property(a => a.UserAgent).HasMaxLength(512);

        // Kept within 2000 characters so it maps to an ordinary string column on every
        // provider (Oracle NVARCHAR2 tops out at 2000) rather than a LOB.
        builder.Property(a => a.Details).HasMaxLength(2000);

        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => new { a.UserId, a.CreatedAt });
        builder.HasIndex(a => new { a.BranchId, a.CreatedAt });
    }
}
