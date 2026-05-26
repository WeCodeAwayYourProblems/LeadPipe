using LeadPipe.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.Database.Configuration;

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
        corn.Property(c => c.UtmSource)
            .IsRequired(false)
            .HasMaxLength(45);
        corn.Property(c => c.UtmMedium)
            .IsRequired(false)
            .HasMaxLength(45);
        corn.Property(c => c.UtmCampaign)
            .IsRequired(false)
            .HasMaxLength(45); 
        corn.Property(c => c.UtmContent)
            .IsRequired(false)
            .HasMaxLength(45);
        corn.Property(c => c.UtmTerm)
            .IsRequired(false)
            .HasMaxLength(45);
    }
}