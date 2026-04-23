using LeadPipe.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.Database.Configuration;

internal sealed class PlumbingConfiguration : IEntityTypeConfiguration<PlumbingEntity>
{
    public void Configure(EntityTypeBuilder<PlumbingEntity> builder)
    {
        var plumb = builder
            .ToTable(TableNames.PlumbingEntitiesName);
        plumb.HasKey(p => p.Id);
        plumb.Property(p => p.Id).ValueGeneratedOnAdd(); // Internal Id
        plumb.HasIndex(p => new
        {
            p.PhoneNumber,
            p.UnixDate,
            p.Source,
            p.MetaData
        }).IsUnique();
        plumb.HasIndex(p => p.Date);
        plumb.Property(p => p.Source)
            .HasConversion<string>()
            .IsRequired();
        plumb.Property(c => c.PhoneNumber)
            .HasConversion(PlumbingConversionsHelper.PhoneNumberAndLongConversion)
            .Metadata.SetValueComparer(PlumbingConversionsHelper.PhoneNumberComparer);

        plumb.HasMany(p => p.PhoneNumbers)
            .WithOne(pn => pn.Plumbing)
            .HasForeignKey(pn => pn.PlumbingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
