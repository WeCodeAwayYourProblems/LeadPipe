using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.MySql.Context.Configuration;

internal sealed class SandMySqlEntityConfiguration(IMySqlSettings settings)
    : IEntityTypeConfiguration<SandMySqlEntity>
{
    private readonly IMySqlSettings _settings = settings;

    public void Configure(EntityTypeBuilder<SandMySqlEntity> entity)
    {
        entity.ToTable(_settings.SandTableName!, schema: _settings.Schema1!);

        entity.HasKey(x => x.subscriptionID);

        entity.HasOne(sand => sand.offerman)
            .WithMany(offerman => offerman.SandEntities)
            .HasForeignKey(sand => sand.officeID)
            .HasPrincipalKey(offerman => offerman.officeID)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

