using LeadPipe.Infrastructure.MySql.Context.Configuration;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.MySql.Context;

public sealed class MySqlSchema2Context(
    DbContextOptions<MySqlSchema2Context> options,
    IMySqlSettings settings
    ) : MySqlBaseContext(options, settings)
{
    protected override void ApplyConfigurations(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(
            new CaliperMySqlEntityConfiguration(Settings.Schema2!));

        modelBuilder.ApplyConfiguration(
            new SummaryMySqlEntityConfiguration(Settings.Schema2!));

        modelBuilder.ApplyConfiguration(
            new TranscriptionMySqlEntityConfiguration(Settings.Schema2!));

        modelBuilder.ApplyConfiguration(
            new CustomerMySqlEntityConfiguration(Settings.Schema1!));

        modelBuilder.ApplyConfiguration(
            new SubMySqlEntityConfiguration(Settings.Schema1!));
    }

    // BLOCK ALL WRITE OPERATIONS
    public override int SaveChanges() =>
        throw new InvalidOperationException("SaveChanges is disabled. MySQL database is read-only.");

    public override int SaveChanges(bool acceptAllChangesOnSuccess) =>
        throw new InvalidOperationException("SaveChanges is disabled. MySQL database is read-only.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("SaveChangesAsync is disabled. MySQL database is read-only.");

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("SaveChangesAsync is disabled. MySQL database is read-only.");
}
