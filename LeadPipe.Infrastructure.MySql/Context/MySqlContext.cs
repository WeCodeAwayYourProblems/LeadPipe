using LeadPipe.Infrastructure.Entity.MySql;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.MySql.Context;

public class MySqlContext(DbContextOptions<MySqlContext> options) : DbContext(options)
{
    public DbSet<CallMySqlEntity> Calls { get; set; }
    public DbSet<CustardMySqlEntity> Customers { get; set; }
    public DbSet<SubMySqlEntity> Subscriptions { get; set; }
    public DbSet<SummaryMySqlEntity> Summaries { get; set; }
    public DbSet<CustomerCallMySqlEntity> CustomerCalls { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TABLE NAMES
        modelBuilder.Entity<CallMySqlEntity>().ToTable("calls");
        modelBuilder.Entity<CustardMySqlEntity>().ToTable("customer");
        modelBuilder.Entity<SubMySqlEntity>().ToTable("subscription");
        modelBuilder.Entity<SummaryMySqlEntity>().ToTable("summary");
        modelBuilder.Entity<CustomerCallMySqlEntity>().ToTable("customer_call_link");

        // PRIMARY KEYS
        modelBuilder.Entity<CallMySqlEntity>()
            .HasKey(x => x.call_id);
        modelBuilder.Entity<CustardMySqlEntity>()
            .HasKey(x => x.customerID);
        modelBuilder.Entity<SubMySqlEntity>()
            .HasKey(x => x.subscriptionID);

        // Summary PK = CallId (one-to-one)
        modelBuilder.Entity<SummaryMySqlEntity>()
            .HasKey(x => x.call_id);

        modelBuilder.Entity<CustomerCallMySqlEntity>()
            .HasKey(x => x.Id);

        // CUSTOMER → SUBSCRIPTIONS (1:M)
        modelBuilder.Entity<SubMySqlEntity>()
            .HasOne<CustardMySqlEntity>()
            .WithMany()
            .HasForeignKey(x => x.customerID)
            .OnDelete(DeleteBehavior.NoAction);

        // CALL ↔ SUMMARY (1:1)
        modelBuilder.Entity<CallMySqlEntity>()
            .HasOne<SummaryMySqlEntity>()
            .WithOne()
            .HasForeignKey<SummaryMySqlEntity>(x => x.call_id);

        // CALL ↔ CUSTOMER (M:N via join table)
        modelBuilder.Entity<CustomerCallMySqlEntity>()
            .HasOne(x => x.Call)
            .WithMany()
            .HasForeignKey(x => x.CallId);
        modelBuilder.Entity<CustomerCallMySqlEntity>()
            .HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId);

        // POMELO: DateOnly handling
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
