using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.MySql.Context;

public abstract class MySqlBaseContext : DbContext
{
    protected readonly IMySqlSettings Settings;

    protected MySqlBaseContext(
        DbContextOptions options,
        IMySqlSettings settings
    ) : base(options)
    {
        Settings = settings;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

#pragma warning disable IDE0079
    public DbSet<CallMySqlEntity> Calls => Set<CallMySqlEntity>();
    public DbSet<CustomerMySqlEntity> Customers => Set<CustomerMySqlEntity>();
    public DbSet<SubMySqlEntity> Subscriptions => Set<SubMySqlEntity>();
    public DbSet<SummaryMySqlEntity> Summaries => Set<SummaryMySqlEntity>();
    public DbSet<TranscriptionMySqlEntity> Transcriptions => Set<TranscriptionMySqlEntity>();
    public DbSet<CornMySqlEntity> Corn => Set<CornMySqlEntity>();
#pragma warning restore IDE0079

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ApplyConfigurations(modelBuilder);
    }

    protected abstract void ApplyConfigurations(ModelBuilder modelBuilder);

    // Read-only enforcement
    public override int SaveChanges() =>
        throw new InvalidOperationException("MySQL database is read-only.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("MySQL database is read-only.");
}
