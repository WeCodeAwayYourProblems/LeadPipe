using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.Sqlite.Context.Configuration;

internal sealed class SyncStampConfiguration : IEntityTypeConfiguration<SyncStampEntity>
{
    public void Configure(EntityTypeBuilder<SyncStampEntity> builder)
    {
        var stmp = builder
            .ToTable(TableNames.SyncStampName);
        stmp.HasKey(x => x.Id);
        stmp.Property(x => x.Id)
            .ValueGeneratedOnAdd();
        stmp.Property(x => x.Key)
            .HasConversion(
                v => v.Value,
                v => SyncKey.From(v))
            .IsRequired();
        stmp.Property(x => x.Source)
            .HasConversion<string>()
            .IsRequired(false);
        stmp.Property(x => x.SuccessState)
            .IsRequired();
        stmp.Property(x => x.UnixSyncUtc)
            .IsRequired();
        stmp.HasIndex(x => new { x.Key, x.Source })
            .IsUnique()
            .HasFilter($"{nameof(SyncStampEntity.Source)} IS NOT NULL");
    }
}
