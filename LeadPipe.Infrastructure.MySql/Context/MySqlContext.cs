using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.MySql.Context;

public class MySqlContext(DbContextOptions<MySqlContext> options, IMySqlSettings settings) : DbContext(options)
{
    private readonly IMySqlSettings _settings = settings;

    // Tables
    public DbSet<CallMySqlEntity> Calls { get; set; }
    public DbSet<CustardMySqlEntity> Customers { get; set; }
    public DbSet<SubMySqlEntity> Subscriptions { get; set; }
    public DbSet<SummaryMySqlEntity> Summaries { get; set; }
    public DbSet<CustomerCallMySqlEntity> CustomerCalls { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TABLE NAMES
        modelBuilder.Entity<CallMySqlEntity>()
            .ToTable("calls", schema: _settings.Schema2);
        modelBuilder.Entity<SummaryMySqlEntity>()
            .ToTable("summary", schema: _settings.Schema2);
        modelBuilder.Entity<CustardMySqlEntity>()
            .ToTable("customer", schema: _settings.Schema1);
        modelBuilder.Entity<SubMySqlEntity>()
            .ToTable("subscription", schema: _settings.Schema1);

        // PRIMARY KEYS
        modelBuilder.Entity<CallMySqlEntity>()
            .HasKey(x => x.call_id);
        modelBuilder.Entity<CustardMySqlEntity>()
            .HasKey(x => x.customerID);
        modelBuilder.Entity<SubMySqlEntity>()
            .HasKey(x => x.subscriptionID);
        modelBuilder.Entity<SummaryMySqlEntity>()
            .HasKey(x => x.call_id);
        modelBuilder.Entity<CustomerCallMySqlEntity>()
            .HasKey(x => x.Id);

        // RELATIONSHIPS
        // 1) Customer (1) to Subscriptions (M)
        modelBuilder.Entity<SubMySqlEntity>()
            .HasOne<CustardMySqlEntity>()
            .WithMany()
            .HasForeignKey(x => x.customerID)
            .OnDelete(DeleteBehavior.NoAction);

        // 2) Call (1) to Summary (1)
        modelBuilder.Entity<CallMySqlEntity>()
            .HasOne<SummaryMySqlEntity>()
            .WithOne()
            .HasForeignKey<SummaryMySqlEntity>(x => x.call_id);

        // 3) Customer (M) to Call (M) — via CustomerCallMySqlEntity
        modelBuilder.Entity<CustomerCallMySqlEntity>()
            .HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<CustomerCallMySqlEntity>()
            .HasOne(x => x.Call)
            .WithMany()
            .HasForeignKey(x => x.CallId)
            .OnDelete(DeleteBehavior.NoAction);

        // Optional indexes for performance
        modelBuilder.Entity<CustomerCallMySqlEntity>()
            .HasIndex(x => new { x.CustomerId, x.CallId });
        modelBuilder.Entity<CustomerCallMySqlEntity>()
            .HasIndex(x => x.MatchingPhone);
    }
}
