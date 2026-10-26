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

        // Columns of the same row. The national number is required, so a row without one has no card.
        builder.OwnsOne(a => a.IdentityCard, card =>
        {
            card.Property(a => a.NationalNumber).HasMaxLength(10).IsRequired();
            card.Property(a => a.ArabicName).HasMaxLength(256).IsRequired();
            card.Property(a => a.EnglishName).HasMaxLength(256).IsRequired();
            card.Property(a => a.Gender).HasMaxLength(32);
            card.Property(a => a.BloodType).HasMaxLength(8);
            card.Property(a => a.MotherName).HasMaxLength(256);
            card.Property(a => a.RegistryNumber).HasMaxLength(32);
            card.Property(a => a.Residence).HasMaxLength(256);
            card.Property(a => a.CardNumber).HasMaxLength(32);

            card.HasIndex(a => a.NationalNumber);
        });

        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => new { a.UserId, a.CreatedAt });
        builder.HasIndex(a => new { a.BranchId, a.CreatedAt });
    }
}
