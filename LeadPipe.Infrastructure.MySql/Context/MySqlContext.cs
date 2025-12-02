using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.MySql.Context;

public class MySqlContext(DbContextOptions<MySqlContext> options, IMySqlSettings settings) : DbContext(options)
{
    private readonly IMySqlSettings _settings = settings;

    public DbSet<CallMySqlEntity> Calls { get; set; }
    public DbSet<CustardMySqlEntity> Customers { get; set; }
    public DbSet<SubMySqlEntity> Subscriptions { get; set; }
    public DbSet<SummaryMySqlEntity> Summaries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Table Names (schemas differ)
        modelBuilder.Entity<CallMySqlEntity>().ToTable("calls", schema: _settings.Schema2);
        modelBuilder.Entity<CustardMySqlEntity>().ToTable("customer", schema: _settings.Schema1);
        modelBuilder.Entity<SubMySqlEntity>().ToTable("subscription", schema: _settings.Schema1);
        modelBuilder.Entity<SummaryMySqlEntity>().ToTable("summary", schema: _settings.Schema1);

        // Primary Keys
        modelBuilder.Entity<CallMySqlEntity>()
            .HasKey(x => x.call_id);
        modelBuilder.Entity<CustardMySqlEntity>()
            .HasKey(x => x.customerID);
        modelBuilder.Entity<SubMySqlEntity>()
            .HasKey(x => x.subscriptionID);
        modelBuilder.Entity<SummaryMySqlEntity>()
            .HasKey(x => x.call_id);

        // Customer to Subscriptions (1:M)
        modelBuilder.Entity<SubMySqlEntity>()
            .HasOne<CustardMySqlEntity>()
            .WithMany()
            .HasForeignKey(x => x.customerID)
            .OnDelete(DeleteBehavior.NoAction);

        // Call to Summary(1:1)
        modelBuilder.Entity<CallMySqlEntity>()
            .HasOne<SummaryMySqlEntity>()
            .WithOne()
            .HasForeignKey<SummaryMySqlEntity>(x => x.call_id);

        // DateOnly properties in Pomelo
        modelBuilder.Entity<SubMySqlEntity>()
            .Property(x => x.dateReactived)
            .HasColumnType("date");
        modelBuilder.Entity<SubMySqlEntity>()
            .Property(x => x.dateAddedDate)
            .HasColumnType("date");
        modelBuilder.Entity<SubMySqlEntity>()
            .Property(x => x.customDate)
            .HasColumnType("date");
    }
}
