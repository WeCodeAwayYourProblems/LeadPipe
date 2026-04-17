using LeadPipe.Infrastructure.Entity.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.Database.Configuration;

internal sealed class SyncStateConfiguration: IEntityTypeConfiguration<SyncStateEntity>
{
    public void Configure(EntityTypeBuilder<SyncStateEntity> builder)
    {
        var sync = builder.ToTable(TableNames.SyncStateName);
        sync.HasKey(x => x.Id);
        sync.Property(x => x.Id).ValueGeneratedOnAdd(); // Internal Id
        sync.HasIndex(x => x.BusinessId)
            .IsUnique();
        sync.Property(x => x.BusinessId)
            .HasConversion(
                v => v.Value,
                v => BusinessId.From(v))
            .IsRequired();
    }
}
