using LeadPipe.Infrastructure.Entity.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.Sqlite.Context.Configuration;

internal sealed class SandConfiguration : IEntityTypeConfiguration<SandEntity>
{
    public void Configure(EntityTypeBuilder<SandEntity> builder)
    {
        var sub = builder
            .ToTable(TableNames.SandEntitiesName);
        sub.HasOne(s => s.CustardEntity)
            .WithMany(c => c.SandEntities)
            .HasForeignKey(s => s.CustardId)
            .OnDelete(DeleteBehavior.Cascade);
        sub.HasKey(s => s.Id);
        sub.Property(s => s.Id).ValueGeneratedNever(); // External id
        sub.Property(s => s.Type);
    }
}