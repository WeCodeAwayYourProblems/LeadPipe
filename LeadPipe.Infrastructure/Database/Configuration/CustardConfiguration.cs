using LeadPipe.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.Database.Configuration;

internal sealed class CustardConfiguration : IEntityTypeConfiguration<CustardEntity>
{
    public void Configure(EntityTypeBuilder<CustardEntity> builder)
    {
        builder.ToTable(TableNames.CustardEntitiesName);
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever(); // External id
        builder.HasMany(c => c.SandEntities)
            .WithOne(s => s.CustardEntity)
            .HasForeignKey(s => s.CustardId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(c => c.PhoneNumber);
        builder.HasIndex(c => c.PhoneNumber2);
        builder.Property(c => c.PhoneNumber)
            .HasConversion(PlumbingConversionsHelper.PhoneNumberAndLongConversion)
            .Metadata.SetValueComparer(PlumbingConversionsHelper.PhoneNumberComparer);
        builder.Property(c => c.PhoneNumber2)
            .HasConversion(PlumbingConversionsHelper.PhoneNumberNullableConverter)
            .Metadata.SetValueComparer(PlumbingConversionsHelper.NullablePhoneNumberComparer);
        builder.Ignore(c => c.CancelDate);
    }
}