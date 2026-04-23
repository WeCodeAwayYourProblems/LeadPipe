using LeadPipe.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.Database.Configuration;

internal sealed class PlumbingPhoneNumberConfiguration : IEntityTypeConfiguration<PlumbingPhoneNumber>
{
    public void Configure(EntityTypeBuilder<PlumbingPhoneNumber> builder)
    {
        var phone = builder.ToTable(TableNames.PlumbingPhoneNumbersName);
        phone.HasKey(p => p.Id);
        phone.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        var pNumber = phone.Property(p => p.PhoneNumber);
        pNumber.HasConversion(PlumbingConversionsHelper.PhoneNumberAndLongConversion);
        pNumber.Metadata.SetValueComparer(PlumbingConversionsHelper.PhoneNumberComparer);
        pNumber.IsRequired();

        // Foreign key
        phone.Property(p => p.PlumbingId)
            .IsRequired();

        phone.HasIndex(p => new { p.PlumbingId, p.PhoneNumber })
            .IsUnique();

        phone.HasOne(p => p.Plumbing)
            .WithMany(p => p.PhoneNumbers)
            .HasForeignKey(p => p.PlumbingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}