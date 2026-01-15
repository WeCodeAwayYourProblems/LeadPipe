using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.MySql.Context.Configuration;

internal sealed class CustardMySqlEntityConfiguration(IMySqlSettings settings)
        : IEntityTypeConfiguration<CustardMySqlEntity>
{
    private readonly IMySqlSettings _settings = settings;

    public void Configure(EntityTypeBuilder<CustardMySqlEntity> entity)
    {
        entity.ToTable(_settings.CustardTableName!, schema: _settings.Schema1!);
        entity.HasKey(x => x.customerID);

        entity.HasMany(c => c.subscriptions)
              .WithOne(s => s.customer)
              .HasForeignKey(s => s.customerID)
              .OnDelete(DeleteBehavior.NoAction);
    }
}
