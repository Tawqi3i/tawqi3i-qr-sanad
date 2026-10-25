namespace Tawqi3i.QrSanad.Persistence.Sql.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(a => a.SecurityStamp).HasMaxLength(64).IsRequired();
        builder.Property(a => a.CreatedByIp).HasMaxLength(64);
        builder.Property(a => a.RevokedByIp).HasMaxLength(64);
        builder.Property(a => a.UserAgent).HasMaxLength(512);
        builder.Property(a => a.RevokeReason).HasMaxLength(64);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.TokenHash).IsUnique();
        builder.HasIndex(a => new { a.UserId, a.RevokedAt });
    }
}
