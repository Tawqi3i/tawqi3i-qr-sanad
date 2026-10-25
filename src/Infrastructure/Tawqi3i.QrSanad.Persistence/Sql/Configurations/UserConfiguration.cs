namespace Tawqi3i.QrSanad.Persistence.Sql.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.UserName).HasMaxLength(64).IsRequired();
        builder.Property(a => a.NormalizedUserName).HasMaxLength(64).IsRequired();
        builder.Property(a => a.Email).HasMaxLength(256).IsRequired();
        builder.Property(a => a.NormalizedEmail).HasMaxLength(256).IsRequired();
        builder.Property(a => a.DisplayName).HasMaxLength(128).IsRequired();
        builder.Property(a => a.PhoneNumber).HasMaxLength(32);
        builder.Property(a => a.UserType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(a => a.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(a => a.SecurityStamp).HasMaxLength(64).IsRequired();
        builder.Property(a => a.ConcurrencyStamp).HasMaxLength(64).IsRequired().IsConcurrencyToken();
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(32).IsRequired();

        builder.HasOne(a => a.Branch)
            .WithMany(a => a.Users)
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique across deleted users too: a deleted user's username and email stay reserved,
        // so audit entries keep pointing at one identity.
        builder.HasIndex(a => a.NormalizedUserName).IsUnique();
        builder.HasIndex(a => a.NormalizedEmail).IsUnique();
        builder.HasIndex(a => new { a.BranchId, a.UserType });
        builder.HasIndex(a => a.CreatedAt);
    }
}
