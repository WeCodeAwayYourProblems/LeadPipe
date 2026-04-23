using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.MySql.Context.Configuration;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.MySql.Context;

public abstract class MySqlBaseContext : DbContext
{
    protected readonly IMySqlSettings Settings;
    protected readonly bool _allowWrites;

    protected MySqlBaseContext(
        DbContextOptions options,
        IMySqlSettings settings,
        bool allowWrites = false
    ) : base(options)
    {
        Settings = settings;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        _allowWrites = allowWrites;
    }

#pragma warning disable IDE0079
    public DbSet<CaliperMySqlEntity> Calipers => Set<CaliperMySqlEntity>();
    public DbSet<CustardMySqlEntity> Custards => Set<CustardMySqlEntity>();
    public DbSet<SandMySqlEntity> Subscriptions => Set<SandMySqlEntity>();
    public DbSet<OffermanMySqlEntity> Offermans => Set<OffermanMySqlEntity>();
    public DbSet<SummaryMySqlEntity> Summaries => Set<SummaryMySqlEntity>();
    public DbSet<TranscriptionMySqlEntity> Transcriptions => Set<TranscriptionMySqlEntity>();
    public DbSet<CornMySqlEntity> Corns => Set<CornMySqlEntity>();
#pragma warning restore IDE0079

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CornMySqlEntityConfiguration(Settings));
        modelBuilder.ApplyConfiguration(new CustardMySqlEntityConfiguration(Settings));
        modelBuilder.ApplyConfiguration(new SandMySqlEntityConfiguration(Settings));
        modelBuilder.ApplyConfiguration(new OffermanMySqlEntityConfiguration(Settings));
        modelBuilder.ApplyConfiguration(new CaliperMySqlEntityConfiguration(Settings));
        modelBuilder.ApplyConfiguration(new SummaryMySqlEntityConfiguration(Settings));
        modelBuilder.ApplyConfiguration(new TranscriptionMySqlEntityConfiguration(Settings));
    }

    // BLOCK ALL WRITE OPERATIONS

    public override int SaveChanges()
        => _allowWrites
        ? base.SaveChanges()
        : throw new InvalidOperationException("SaveChanges is disabled. MySQL database is read-only.");

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
        => _allowWrites
        ? base.SaveChanges(acceptAllChangesOnSuccess)
        : throw new InvalidOperationException("SaveChanges is disabled. MySQL database is read-only.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _allowWrites
        ? base.SaveChangesAsync(cancellationToken)
        : throw new InvalidOperationException("SaveChangesAsync is disabled. MySQL database is read-only.");

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        => _allowWrites
        ? base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken)
        : throw new InvalidOperationException("SaveChangesAsync is disabled. MySQL database is read-only.");

}
