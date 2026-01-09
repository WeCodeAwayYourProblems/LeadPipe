using LeadPipe.Infrastructure.Entity.MySql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.MySql.Context.Configuration;

internal sealed class SandMySqlEntityConfiguration(string schema)
        : IEntityTypeConfiguration<SandMySqlEntity>
{
    private readonly string _schema = schema;

    public void Configure(EntityTypeBuilder<SandMySqlEntity> entity)
    {
        entity.ToTable("subscription", schema: _schema);
        entity.HasKey(x => x.subscriptionID);
    }
}
