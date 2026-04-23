using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.MySql.Context.Configuration;

internal sealed class SummaryMySqlEntityConfiguration(IMySqlSettings settings)
        : IEntityTypeConfiguration<SummaryMySqlEntity>
{
    private readonly IMySqlSettings _settings = settings;

    public void Configure(EntityTypeBuilder<SummaryMySqlEntity> entity)
    {
        entity.ToTable("call_summary", schema: _settings.Schema2!);
        entity.HasKey(x => x.call_id);
    }
}
