using LeadPipe.Infrastructure.Entity.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.Database.Configuration;

internal sealed class CaliperConfiguration : IEntityTypeConfiguration<CaliperEntity>
{
    public void Configure(EntityTypeBuilder<CaliperEntity> builder)
    {
        var caliper = builder
            .ToTable(TableNames.CaliperEntitiesName);
        caliper.HasKey(c => c.Id);
        caliper.Property(c => c.Id).ValueGeneratedNever(); // External id
        caliper.HasIndex(c => c.PhoneNumber);
        caliper.Property(c => c.Date).IsRequired();
        caliper.Property(c => c.UnixDate).IsRequired();
        caliper.HasIndex(c => c.Date);
        caliper.Property(c => c.PhoneNumber)
            .HasConversion(PlumbingConversionsHelper.PhoneNumberAndLongConversion)
            .Metadata.SetValueComparer(PlumbingConversionsHelper.PhoneNumberComparer);
    }
}
