using LeadPipe.Infrastructure.Entity.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.Sqlite.Context;

public sealed class PlumbingContext(DbContextOptions<PlumbingContext> options) : DbContext(options)
{
    #region DbSets
#pragma warning disable IDE0079
    // Entities
    public DbSet<SyncStateEntity> SyncState { get; set; }
    public DbSet<CaliperEntity> CaliperEntities { get; set; }
    public DbSet<CornEntity> CornEntities { get; set; }
    public DbSet<PlumbingEntity> PlumbingEntities { get; set; }
    public DbSet<CustardEntity> CustardEntities { get; set; }
    public DbSet<SandEntity> SandEntities { get; set; }

    // Links
    public DbSet<CornCaliperLink> CornCaliperLinks { get; set; }
    public DbSet<CornPlumbingLink> CornPlumbingLinks { get; set; }
    public DbSet<PlumbingCaliperLink> PlumbingCaliperLinks { get; set; }
    public DbSet<CustardCaliperLink> CustardCaliperLinks { get; set; }
    public DbSet<CustardCornLink> CustardCornLinks { get; set; }
    public DbSet<CustardPlumbingLink> CustardPlumbingLinks { get; set; }
    public DbSet<SandCaliperLink> SandCaliperLinks { get; set; }
    public DbSet<SandCornLink> SandCornLinks { get; set; }
    public DbSet<SandPlumbingLink> SandPlumbingLinks { get; set; }

#pragma warning restore IDE0079
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // **************************************
        // Entities
        // **************************************

        #region Entities

        // Sync State Entity
        var sync = modelBuilder.Entity<SyncStateEntity>()
            .ToTable("SyncState");
        sync.HasKey(x => x.Id);
        sync.HasIndex(x => x.Id)
            .IsUnique();
        sync.Property(x => x.Id).ValueGeneratedNever(); // External id

        // CaliperEntity
        var caliper = modelBuilder.Entity<CaliperEntity>()
            .ToTable("CaliperEntities");
        caliper.HasKey(c => c.Id);
        caliper.Property(c => c.Id).ValueGeneratedNever(); // External id
        caliper.HasIndex(c => c.PhoneNumber);
        caliper.Property(c => c.CaliperDate).IsRequired();
        caliper.Property(c => c.UnixDate).IsRequired();
        caliper.HasIndex(c => new { c.PhoneNumber, c.CaliperDate }).IsUnique();

        // CornEntity
        var corn = modelBuilder.Entity<CornEntity>()
            .ToTable("CornEntities");
        corn.HasKey(c => c.Id);
        corn.Property(c => c.Id).ValueGeneratedNever(); // External id
        corn.HasIndex(c => c.PhoneNumber);
        corn.HasIndex(c => new { c.PhoneNumber, c.Source }).IsUnique();
        corn.Property(c => c.Source)
            .HasConversion<string>()
            .IsRequired();
        corn.Property(c => c.MetaData)
            .IsRequired();
        corn.Property(c => c.Payload)
            .IsRequired();
        corn.Property(c => c.Date)
            .IsRequired();
        corn.Property(c => c.UnixDate)
            .IsRequired();

        // PlumbingEntity
        var plumb = modelBuilder.Entity<PlumbingEntity>()
            .ToTable("PlumbingEntities");
        plumb.HasKey(p => p.Id);
        plumb.Property(p => p.Id).ValueGeneratedOnAdd();
        plumb.HasIndex(p => p.PhoneNumber);
        plumb.HasIndex(p => new { p.PhoneNumber, p.Source }).IsUnique();
        plumb.Property(p => p.Source)
            .HasConversion<string>()
            .IsRequired();

        // Custard Entity
        var custard = modelBuilder.Entity<CustardEntity>();
        custard.ToTable("CustardEntities", t =>
        {
            t.HasCheckConstraint(
                "CK_Custard_PhoneNumber2_NotZero",
                "PhoneNumber2 <> 0"
            );
        });
        custard.HasKey(c => c.Id);
        custard.Property(c => c.Id).ValueGeneratedNever(); // External id
        custard.HasOne(c => c.Sand)
            .WithMany(s => s.CustardEntities)
            .HasForeignKey(c => c.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
        custard.HasIndex(c => c.PhoneNumber);
        custard.HasIndex(c => c.PhoneNumber2);
        custard.HasIndex(c => c.SubscriptionId);

        // SandEntity
        var sub = modelBuilder.Entity<SandEntity>()
            .ToTable("SandEntities");
        sub.HasKey(s => s.Id);
        sub.Property(s => s.Id).ValueGeneratedNever(); // External id
        sub.Property(s => s.Type);
        #endregion

        // **************************************
        // Links
        // **************************************

        #region PlumbingCaliperLinks, CornCaliperLinks, CornPlumbingLinks
        // PlumbingCaliperLink
        var plumbCaliper = modelBuilder.Entity<PlumbingCaliperLink>()
            .ToTable("PlumbingCaliperLinks");
        plumbCaliper.HasKey(pc => pc.Id);
        plumbCaliper.Property(pc => pc.Id).ValueGeneratedOnAdd();
        plumbCaliper.HasIndex(pc => pc.PlumbingId);
        plumbCaliper.HasIndex(pc => pc.CaliperId);
        plumbCaliper.HasIndex(l => new { l.PlumbingId, l.CaliperId }).IsUnique();
        plumbCaliper.HasOne(pc => pc.PlumbingEntity)
            .WithMany(p => p.PlumbingCaliperLinks)
            .HasForeignKey(pc => pc.PlumbingId)
            .OnDelete(DeleteBehavior.Cascade);
        plumbCaliper.HasOne(pc => pc.CaliperEntity)
            .WithMany(c => c.PlumbingCaliperLinks)
            .HasForeignKey(pc => pc.CaliperId)
            .OnDelete(DeleteBehavior.Cascade);

        // CornCaliperLink
        var cornCaliper = modelBuilder.Entity<CornCaliperLink>()
            .ToTable("CornCaliperLinks");
        cornCaliper.HasKey(l => l.Id);
        cornCaliper.Property(l => l.Id).ValueGeneratedOnAdd();
        cornCaliper.HasIndex(l => l.CornId);
        cornCaliper.HasIndex(l => l.CaliperId);
        cornCaliper.HasIndex(l => new { l.CornId, l.CaliperId }).IsUnique();
        cornCaliper.HasOne(l => l.CornEntity)
            .WithMany(c => c.CornCaliperLinks)
            .HasForeignKey(l => l.CornId)
            .OnDelete(DeleteBehavior.Cascade);
        cornCaliper.HasOne(l => l.CaliperEntity)
            .WithMany(c => c.CornCaliperLinks)
            .HasForeignKey(l => l.CaliperId)
            .OnDelete(DeleteBehavior.Cascade);
        cornCaliper.Property(l => l.MatchingPhone)
            .IsRequired();

        // CornPlumbingLink
        var cornPlumb = modelBuilder.Entity<CornPlumbingLink>()
            .ToTable("CornPlumbingLinks");
        cornPlumb.HasKey(l => l.Id);
        cornPlumb.Property(l => l.Id).ValueGeneratedOnAdd();
        cornPlumb.HasIndex(l => l.CornId);
        cornPlumb.HasIndex(l => l.PlumbingId);
        cornPlumb.HasIndex(l => new { l.CornId, l.PlumbingId }).IsUnique();
        cornPlumb.HasOne(l => l.CornEntity)
            .WithMany(c => c.CornPlumbingLinks)
            .HasForeignKey(l => l.CornId)
            .OnDelete(DeleteBehavior.Cascade);
        cornPlumb.HasOne(l => l.PlumbingEntity)
            .WithMany(p => p.CornPlumbingLinks)
            .HasForeignKey(l => l.PlumbingId)
            .OnDelete(DeleteBehavior.Cascade);
        cornPlumb.Property(l => l.MatchingPhone)
            .IsRequired();
        #endregion

        #region CUSTARD Links
        // Custard Caliper Links
        var custardCaliper = modelBuilder.Entity<CustardCaliperLink>();
        custardCaliper.ToTable("CustardCaliperLinks", t =>
        {
            t.HasCheckConstraint(
                "CK_CustardCaliper_MatchingPhone",
                "MatchingPhone <> 0");
        });
        custardCaliper.HasKey(l => l.Id);
        custardCaliper.HasIndex(l => l.CustardId);
        custardCaliper.HasIndex(l => l.CaliperId);
        custardCaliper.Property(l => l.Id)
            .ValueGeneratedOnAdd();
        custardCaliper.HasIndex(l => new { l.CustardId, l.CaliperId }).IsUnique();
        custardCaliper.Property(l => l.CustardId).IsRequired();
        custardCaliper.Property(l => l.CaliperId).IsRequired();
        custardCaliper.HasOne(l => l.Custard)
          .WithMany(c => c.CustardCaliperLinks)
          .HasForeignKey(l => l.CustardId)
          .OnDelete(DeleteBehavior.Cascade);
        custardCaliper.HasOne(l => l.Caliper)
          .WithMany(c => c.CustardCaliperLinks)
          .HasForeignKey(l => l.CaliperId)
          .OnDelete(DeleteBehavior.Cascade);
        custardCaliper.Property(l => l.MatchingPhone).IsRequired();

        // Custard Corn Links
        var custardCorn = modelBuilder.Entity<CustardCornLink>();
        custardCorn.ToTable("CustardCornLinks", t =>
        {
            t.HasCheckConstraint(
                "CK_CustardCorn_MatchingPhone",
                "MatchingPhone <> 0");
        });
        custardCorn.HasKey(l => l.Id);
        custardCorn.HasIndex(l => l.CustardId);
        custardCorn.HasIndex(l => l.CornId);
        custardCorn.Property(l => l.Id)
            .ValueGeneratedOnAdd();
        custardCorn.HasIndex(l => new { l.CustardId, l.CornId }).IsUnique();
        custardCorn.Property(l => l.CustardId).IsRequired();
        custardCorn.Property(l => l.CornId).IsRequired();
        custardCorn.HasOne(l => l.Custard)
          .WithMany(c => c.CustardCornLinks)
          .HasForeignKey(l => l.CustardId)
          .OnDelete(DeleteBehavior.Cascade);
        custardCorn.HasOne(l => l.Corn)
          .WithMany(c => c.CustardCornLinks)
          .HasForeignKey(l => l.CornId)
          .OnDelete(DeleteBehavior.Cascade);
        custardCorn.Property(l => l.MatchingPhone).IsRequired();

        // Custard Plumbing Links
        var custardPlumbing = modelBuilder.Entity<CustardPlumbingLink>();
        custardPlumbing.ToTable("CustardPlumbingLinks", t =>
        {
            t.HasCheckConstraint(
                "CK_CustardPlumbing_MatchingPhone",
                "MatchingPhone <> 0");
        });
        custardPlumbing.HasKey(l => l.Id);
        custardPlumbing.HasIndex(l => l.CustardId);
        custardPlumbing.HasIndex(l => l.PlumbingId);
        custardPlumbing.Property(l => l.Id)
            .ValueGeneratedOnAdd();
        custardPlumbing.HasIndex(l => new { l.CustardId, l.PlumbingId }).IsUnique();
        custardPlumbing.Property(l => l.CustardId).IsRequired();
        custardPlumbing.Property(l => l.PlumbingId).IsRequired();
        custardPlumbing.HasOne(l => l.Custard)
          .WithMany(c => c.CustardPlumbingLinks)
          .HasForeignKey(l => l.CustardId)
          .OnDelete(DeleteBehavior.Cascade);
        custardPlumbing.HasOne(l => l.Plumbing)
          .WithMany(c => c.CustardPlumbingLinks)
          .HasForeignKey(l => l.PlumbingId)
          .OnDelete(DeleteBehavior.Cascade);
        custardPlumbing.Property(l => l.MatchingPhone).IsRequired();
        #endregion

        #region SAND links
        // SandCaliperLink
        var sandCaliper = modelBuilder.Entity<SandCaliperLink>()
            .ToTable("SandCaliperLinks");
        sandCaliper.HasKey(sc => sc.Id);
        sandCaliper.Property(sc => sc.Id).ValueGeneratedOnAdd();
        sandCaliper.HasIndex(sc => sc.SandId);
        sandCaliper.HasIndex(sc => sc.CaliperId);
        sandCaliper.HasIndex(l => new { l.SandId, l.CaliperId }).IsUnique();
        sandCaliper.HasOne(sc => sc.SandEntity)
            .WithMany(s => s.SandCaliperLinks)
            .HasForeignKey(sc => sc.SandId)
            .OnDelete(DeleteBehavior.Cascade);
        sandCaliper.HasOne(sc => sc.CaliperEntity)
            .WithMany(c => c.SandCaliperLinks)
            .HasForeignKey(sc => sc.CaliperId)
            .OnDelete(DeleteBehavior.Cascade);
        sandCaliper.Property(sc => sc.MatchingPhone).IsRequired();

        // SandCornLink
        var sandCorn = modelBuilder.Entity<SandCornLink>()
            .ToTable("SandCornLinks");
        sandCorn.HasKey(l => l.Id);
        sandCorn.Property(l => l.Id).ValueGeneratedOnAdd();
        sandCorn.HasIndex(l => l.SandId);
        sandCorn.HasIndex(l => l.CornId);
        sandCorn.HasIndex(l => new { l.SandId, l.CornId }).IsUnique();
        sandCorn.HasOne(l => l.SandEntity)
            .WithMany(s => s.SandCornLinks)
            .HasForeignKey(l => l.SandId)
            .OnDelete(DeleteBehavior.Cascade);
        sandCorn.HasOne(l => l.CornEntity)
            .WithMany(c => c.SandCornLinks)
            .HasForeignKey(l => l.CornId)
            .OnDelete(DeleteBehavior.Cascade);
        sandCorn.Property(l => l.MatchingPhone)
            .IsRequired();

        // SandPlumbingLink
        var spLink = modelBuilder.Entity<SandPlumbingLink>()
            .ToTable("SandPlumbingLinks");
        spLink.HasKey(l => l.Id);
        spLink.Property(l => l.Id).ValueGeneratedOnAdd();
        spLink.HasIndex(l => l.SandId);
        spLink.HasIndex(l => l.PlumbingId);
        spLink.HasIndex(l => new { l.SandId, l.PlumbingId }).IsUnique();
        spLink.HasOne(l => l.SandEntity)
            .WithMany(s => s.SandPlumbingLinks)
            .HasForeignKey(l => l.SandId)
            .OnDelete(DeleteBehavior.Cascade);
        spLink.HasOne(l => l.PlumbingEntity)
            .WithMany(p => p.SandPlumbingLinks)
            .HasForeignKey(l => l.PlumbingId)
            .OnDelete(DeleteBehavior.Cascade);
        spLink.Property(l => l.MatchingPhone).IsRequired();
        #endregion
    }
}
