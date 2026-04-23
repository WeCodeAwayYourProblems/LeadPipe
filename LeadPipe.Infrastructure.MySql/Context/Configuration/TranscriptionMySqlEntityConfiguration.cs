using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.MySql.Context.Configuration;

internal sealed class TranscriptionMySqlEntityConfiguration(IMySqlSettings settings)
        : IEntityTypeConfiguration<TranscriptionMySqlEntity>
{
    private readonly IMySqlSettings _settings = settings;

    public void Configure(EntityTypeBuilder<TranscriptionMySqlEntity> entity)
    {
        entity.ToTable("transcriptions", schema: _settings.Schema2!);
        entity.HasKey(x => x.call_id);
    }
}
