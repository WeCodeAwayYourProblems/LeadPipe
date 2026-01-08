using LeadPipe.Infrastructure.Entity.MySql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.MySql.Context.Configuration;

internal sealed class CornMySqlEntityConfiguration(string schema)
    : IEntityTypeConfiguration<CornMySqlEntity>
{
    private readonly string _schema = schema;

    public void Configure(EntityTypeBuilder<CornMySqlEntity> entity)
    {
        entity.ToTable("corn", schema: _schema);

        entity.HasKey(x => x.id);

        entity.Property(x => x.id)
            .ValueGeneratedNever();

        entity.Property(x => x.timestamp)
              .HasColumnType("datetime");
    }
}
