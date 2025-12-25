using LeadPipe.Infrastructure.Entity.MySql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.MySql.Context.Configuration;

internal sealed class SummaryMySqlEntityConfiguration(string schema)
        : IEntityTypeConfiguration<SummaryMySqlEntity>
{
    private readonly string _schema = schema;

    public void Configure(EntityTypeBuilder<SummaryMySqlEntity> entity)
    {
        entity.ToTable("call_summary", schema: _schema);
        entity.HasKey(x => x.call_id);
    }
}
