namespace Tawqi3i.QrSanad.Persistence.Sql.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawqi3i.QrSanad.Common.Domain.Models.V1;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Licensing;

/// <summary>
/// Not seeded: the first row comes from Licensing:Key at startup (LicenseInitializer).
/// </summary>
internal sealed class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Key).HasMaxLength(LicenseRequest.MaxKeyLength).IsRequired();
        builder.Property(a => a.ConcurrencyStamp).HasMaxLength(64).IsRequired().IsConcurrencyToken();
    }
}
