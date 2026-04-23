using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.MySql.Context.Configuration;

internal sealed class CornMySqlEntityConfiguration(IMySqlSettings settings)
    : IEntityTypeConfiguration<CornMySqlEntity>
{
    private readonly IMySqlSettings _settings = settings;
    
    public void Configure(EntityTypeBuilder<CornMySqlEntity> entity)
    {
        entity.ToTable(_settings.CornTableName!, schema: _settings.Schema3!);

        entity.HasKey(x => x.id);

        entity.Property(x => x.id)
            .ValueGeneratedNever();

        entity.Property(x => x.timestamp)
              .HasColumnType("datetime");
    }
}
