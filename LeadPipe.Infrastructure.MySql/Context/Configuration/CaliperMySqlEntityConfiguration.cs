using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.MySql.Context.Configuration;

internal sealed class CaliperMySqlEntityConfiguration(IMySqlSettings settings)
        : IEntityTypeConfiguration<CaliperMySqlEntity>
{
    private readonly IMySqlSettings _settings = settings;

    public void Configure(EntityTypeBuilder<CaliperMySqlEntity> entity)
    {
        entity.ToTable("calls", schema: _settings.Schema2!);
        entity.HasKey(x => x.call_id);

        entity.HasMany(c => c.summaries)
              .WithOne()
              .HasForeignKey(s => s.call_id)
              .OnDelete(DeleteBehavior.NoAction);

        entity.HasMany(c => c.transcriptions)
              .WithOne()
              .HasForeignKey(t => t.call_id)
              .OnDelete(DeleteBehavior.NoAction);
    }
}
