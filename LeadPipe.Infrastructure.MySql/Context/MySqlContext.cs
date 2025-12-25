using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.MySql.Context;

public class MySqlContext : DbContext
{
    private readonly IMySqlSettings _settings;

    public MySqlContext(DbContextOptions<MySqlContext> options, IMySqlSettings settings)
        : base(options)
    {
        _settings = settings;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }
#pragma warning disable IDE0079
    public DbSet<CallMySqlEntity> Calls { get; set; }
    public DbSet<CustomerMySqlEntity> Customers { get; set; }
    public DbSet<SubMySqlEntity> Subscriptions { get; set; }
    public DbSet<SummaryMySqlEntity> Summaries { get; set; }
    public DbSet<TranscriptionMySqlEntity> Transcriptions { get; set; }
#pragma warning restore IDE0079

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // CALL ENTITY
        modelBuilder.Entity<CallMySqlEntity>(entity =>
        {
            entity.ToTable("calls", schema: _settings.Schema2);
            entity.HasKey(x => x.call_id);

            entity.HasMany(c => c.summaries)
                  .WithOne()
                  .HasForeignKey(s => s.call_id)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(c => c.transcriptions)
                  .WithOne()
                  .HasForeignKey(t => t.call_id)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // SUMMARY ENTITY
        modelBuilder.Entity<SummaryMySqlEntity>(entity =>
        {
            entity.ToTable("call_summary", schema: _settings.Schema2);
            entity.HasKey(x => x.call_id);
        });

        // TRANSCRIPTION ENTITY
        modelBuilder.Entity<TranscriptionMySqlEntity>(entity =>
        {
            entity.ToTable("transcriptions", schema: _settings.Schema2);
            entity.HasKey(x => x.call_id);
        });

        // CUSTOMER ENTITY
        modelBuilder.Entity<CustomerMySqlEntity>(entity =>
        {
            entity.ToTable("customer", schema: _settings.Schema1);
            entity.HasKey(x => x.customerID);

            entity.HasMany(c => c.subscriptions)
                  .WithOne(s => s.customer)
                  .HasForeignKey(s => s.customerID)
                  .OnDelete(DeleteBehavior.NoAction);
        });


        // SUBSCRIPTION ENTITY
        modelBuilder.Entity<SubMySqlEntity>(entity =>
        {
            entity.ToTable("subscription", schema: _settings.Schema1);
            entity.HasKey(x => x.subscriptionID);
        });
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
