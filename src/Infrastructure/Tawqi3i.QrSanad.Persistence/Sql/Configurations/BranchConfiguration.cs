namespace Tawqi3i.QrSanad.Persistence.Sql.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;

internal sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Name).HasMaxLength(128).IsRequired();
        builder.Property(a => a.NormalizedName).HasMaxLength(128).IsRequired();
        builder.Property(a => a.Code).HasMaxLength(32);
        builder.Property(a => a.Address).HasMaxLength(256);
        builder.Property(a => a.PhoneNumber).HasMaxLength(32);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(a => a.ConcurrencyStamp).HasMaxLength(64).IsRequired().IsConcurrencyToken();

        // One-to-one: the unique index on AdminUserId is what guarantees one admin per branch
        // and one branch per admin, even under concurrent requests. Nullable unique indexes
        // allow many NULLs on every provider (SQL Server gets an IS NOT NULL filter from EF).
        builder.HasOne(a => a.Admin)
            .WithOne()
            .HasForeignKey<Branch>(a => a.AdminUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.NormalizedName).IsUnique();
        builder.HasIndex(a => a.Code).IsUnique();
    }
}
