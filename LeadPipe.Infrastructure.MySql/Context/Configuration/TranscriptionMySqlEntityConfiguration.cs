using LeadPipe.Infrastructure.Entity.MySql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.MySql.Context.Configuration;

internal sealed class TranscriptionMySqlEntityConfiguration(string schema)
        : IEntityTypeConfiguration<TranscriptionMySqlEntity>
{
    private readonly string _schema = schema;

    public void Configure(EntityTypeBuilder<TranscriptionMySqlEntity> entity)
    {
        entity.ToTable("transcriptions", schema: _schema);
        entity.HasKey(x => x.call_id);
    }
}
