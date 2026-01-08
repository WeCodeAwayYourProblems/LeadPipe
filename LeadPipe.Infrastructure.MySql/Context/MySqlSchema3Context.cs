using LeadPipe.Infrastructure.MySql.Context.Configuration;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.MySql.Context;

public sealed class MySqlSchema3Context(
    DbContextOptions<MySqlSchema3Context> options,
    IMySqlSettings settings
) : MySqlBaseContext(options, settings)
{
    protected override void ApplyConfigurations(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(
            new CornMySqlEntityConfiguration(Settings.Schema3!));
    }

    // BLOCK ALL WRITE OPERATIONS
    public override int SaveChanges() =>
        throw new InvalidOperationException("SaveChanges is disabled. MySQL database is read-only.");

    public override int SaveChanges(bool acceptAllChangesOnSuccess) =>
        throw new InvalidOperationException("SaveChanges is disabled. MySQL database is read-only.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("SaveChangesAsync is disabled. MySQL database is read-only.");

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("SaveChangesAsync is disabled. MySQL database is read-only.");
}
