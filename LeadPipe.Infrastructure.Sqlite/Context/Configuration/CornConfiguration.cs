using LeadPipe.Infrastructure.Entity.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.Sqlite.Context.Configuration;

internal sealed class CornConfiguration : IEntityTypeConfiguration<CornEntity>
{
    public void Configure(EntityTypeBuilder<CornEntity> builder)
    {
        var corn = builder
            .ToTable(TableNames.CornEntitiesName);
        corn.HasKey(c => c.Id);
        corn.Property(c => c.Id).ValueGeneratedNever(); // External id
        corn.HasIndex(c => new { c.PhoneNumber, c.Source });
        corn.Property(c => c.Source)
            .HasConversion<string>()
            .IsRequired();
        corn.Property(c => c.MetaData)
            .IsRequired();
        corn.Property(c => c.Payload)
            .IsRequired();
        corn.Property(c => c.Date)
            .IsRequired();
        corn.Property(c => c.UnixDate)
            .IsRequired();
        corn.Property(c => c.PhoneNumber)
            .HasConversion(PlumbingConversionsHelper.PhoneNumberAndLongConversion)
            .Metadata.SetValueComparer(PlumbingConversionsHelper.PhoneNumberComparer);
    }
}