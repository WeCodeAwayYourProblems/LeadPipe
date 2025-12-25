using LeadPipe.Infrastructure.Entity.MySql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.MySql.Context.Configuration;

internal sealed class CallMySqlEntityConfiguration(string schema)
        : IEntityTypeConfiguration<CallMySqlEntity>
{
    private readonly string _schema = schema;

    public void Configure(EntityTypeBuilder<CallMySqlEntity> entity)
    {
        entity.ToTable("calls", schema: _schema);
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
