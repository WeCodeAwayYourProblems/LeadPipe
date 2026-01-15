using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.MySql.Context.Configuration;

internal sealed class OffermanMySqlEntityConfiguration(IMySqlSettings settings)
    : IEntityTypeConfiguration<OffermanMySqlEntity>
{
    private readonly IMySqlSettings _settings = settings;

    public void Configure(EntityTypeBuilder<OffermanMySqlEntity> entity)
    {
        entity.ToTable("office", schema: _settings.Schema1!);

        entity.HasKey(x => x.officeID);

        entity.HasMany(offerman => offerman.SandEntities)
            .WithOne(sand => sand.offerman)
            .HasForeignKey(sand => sand.officeID)
            .HasPrincipalKey(offerman => offerman.officeID)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
