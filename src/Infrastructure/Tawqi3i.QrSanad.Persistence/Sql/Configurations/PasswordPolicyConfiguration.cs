namespace Tawqi3i.QrSanad.Persistence.Sql.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;

/// <summary>
/// Not seeded through HasData: the initial row comes from Identity:PasswordPolicy at startup
/// (DbInitializer.SeedPasswordPolicy), so each deployment keeps the values it was configured with.
/// </summary>
internal sealed class PasswordPolicyConfiguration : IEntityTypeConfiguration<PasswordPolicy>
{
    public void Configure(EntityTypeBuilder<PasswordPolicy> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();
    }
}
