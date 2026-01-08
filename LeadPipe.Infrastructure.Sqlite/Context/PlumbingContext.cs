using LeadPipe.Infrastructure.Entity.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.Sqlite.Context;

public sealed class PlumbingContext(DbContextOptions<PlumbingContext> options) : DbContext(options)
{
#pragma warning disable IDE0079
    // Entities
    public DbSet<SyncStateEntity> SyncState { get; set; }
    public DbSet<CallEntity> CallEntities { get; set; }
    public DbSet<CornEntity> CornEntities { get; set; }
    public DbSet<PlumbingEntity> PlumbingEntities { get; set; }
    public DbSet<SubsEntity> SubsEntities { get; set; }

    // Links
    public DbSet<CornCallLink> CornCallLinks { get; set; }
    public DbSet<CornPlumbingLink> CornPlumbingLinks { get; set; }
    public DbSet<PlumbingCallLink> PlumbingCallLinks { get; set; }
    public DbSet<SubsCallLink> SubsCallLinks { get; set; }
    public DbSet<SubsCornLink> SubsCornLinks { get; set; }
    public DbSet<SubsPlumbingLink> SubsPlumbingLinks { get; set; }
#pragma warning restore IDE0079

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // **************************************
        // Entities
        // **************************************

        // Sync State Entity
        var sync = modelBuilder.Entity<SyncStateEntity>()
            .ToTable("SyncState");
        sync.HasKey(x => x.Id);
        sync.HasIndex(x => x.Id)
            .IsUnique();
        sync.Property(x => x.Id).ValueGeneratedNever();

        // SubsEntity
        var sub = modelBuilder.Entity<SubsEntity>()
            .ToTable("SubsEntities");
        sub.HasKey(s => s.Id);
        sub.HasIndex(s => s.Number);
        sub.HasIndex(s => s.Number2);
        sub.Property(s => s.Type);

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

        // CallEntity
        var call = modelBuilder.Entity<CallEntity>()
            .ToTable("CallEntities");
        call.HasKey(c => c.Id);
        call.HasIndex(c => c.PhoneNumber);
        call.Property(c => c.CallDate).IsRequired();
        call.Property(c => c.UnixCallDate).IsRequired();
        call.HasIndex(c => new { c.PhoneNumber, c.CallDate }).IsUnique();
        
        // CornEntity
        var corn = modelBuilder.Entity<CornEntity>()
            .ToTable("CornEntities");
        corn.HasKey(c => c.Id);
        corn.Property(c => c.Id).ValueGeneratedOnAdd();
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

        // **************************************
        // Links
        // **************************************

        // SubsPlumbingLink
        var spLink = modelBuilder.Entity<SubsPlumbingLink>()
            .ToTable("SubsPlumbingLinks");
        spLink.HasKey(l => l.Id);
        spLink.Property(l => l.Id).ValueGeneratedOnAdd();
        spLink.HasIndex(l => l.SubsId);
        spLink.HasIndex(l => l.PlumbingId);
        spLink.HasIndex(l => new { l.SubsId, l.PlumbingId }).IsUnique();
        spLink.HasOne(l => l.SubsEntity)
            .WithMany(s => s.SubsPlumbingLinks)
            .HasForeignKey(l => l.SubsId)
            .OnDelete(DeleteBehavior.Cascade);
        spLink.HasOne(l => l.PlumbingEntity)
            .WithMany(p => p.SubsPlumbingLinks)
            .HasForeignKey(l => l.PlumbingId)
            .OnDelete(DeleteBehavior.Cascade);
        spLink.Property(l => l.MatchingSubPhone).IsRequired();

        // SubsCallLink
        var subsCall = modelBuilder.Entity<SubsCallLink>()
            .ToTable("SubsCallLinks");
        subsCall.HasKey(sc => sc.Id);
        subsCall.Property(sc => sc.Id).ValueGeneratedOnAdd();
        subsCall.HasIndex(sc => sc.SubsId);
        subsCall.HasIndex(sc => sc.CallId);
        subsCall.HasIndex(l => new { l.SubsId, l.CallId }).IsUnique();
        subsCall.HasOne(sc => sc.SubsEntity)
            .WithMany(s => s.SubsCallLinks)
            .HasForeignKey(sc => sc.SubsId)
            .OnDelete(DeleteBehavior.Cascade);
        subsCall.HasOne(sc => sc.CallEntity)
            .WithMany(c => c.SubsCallLinks)
            .HasForeignKey(sc => sc.CallId)
            .OnDelete(DeleteBehavior.Cascade);
        subsCall.Property(sc => sc.MatchingNumber).IsRequired();

        // PlumbingCallLink
        var plumbCall = modelBuilder.Entity<PlumbingCallLink>()
            .ToTable("PlumbingCallLinks");
        plumbCall.HasKey(pc => pc.Id);
        plumbCall.Property(pc => pc.Id).ValueGeneratedOnAdd();
        plumbCall.HasIndex(pc => pc.PlumbingId);
        plumbCall.HasIndex(pc => pc.CallId);
        plumbCall.HasIndex(l => new { l.PlumbingId, l.CallId }).IsUnique();
        plumbCall.HasOne(pc => pc.PlumbingEntity)
            .WithMany(p => p.PlumbingCallLinks)
            .HasForeignKey(pc => pc.PlumbingId)
            .OnDelete(DeleteBehavior.Cascade);
        plumbCall.HasOne(pc => pc.CallEntity)
            .WithMany(c => c.PlumbingCallLinks)
            .HasForeignKey(pc => pc.CallId)
            .OnDelete(DeleteBehavior.Cascade);

        // SubsCornLink
        var subsCorn = modelBuilder.Entity<SubsCornLink>()
            .ToTable("SubsCornLinks");
        subsCorn.HasKey(l => l.Id);
        subsCorn.Property(l => l.Id).ValueGeneratedOnAdd();
        subsCorn.HasIndex(l => l.SubsId);
        subsCorn.HasIndex(l => l.CornId);
        subsCorn.HasIndex(l => new { l.SubsId, l.CornId }).IsUnique();
        subsCorn.HasOne(l => l.SubsEntity)
            .WithMany(s => s.SubsCornLinks)
            .HasForeignKey(l => l.SubsId)
            .OnDelete(DeleteBehavior.Cascade);
        subsCorn.HasOne(l => l.CornEntity)
            .WithMany(c => c.SubsCornLinks)
            .HasForeignKey(l => l.CornId)
            .OnDelete(DeleteBehavior.Cascade);
        subsCorn.Property(l => l.MatchingPhone)
            .IsRequired();

        // CornCallLink
        var cornCall = modelBuilder.Entity<CornCallLink>()
            .ToTable("CornCallLinks");
        cornCall.HasKey(l => l.Id);
        cornCall.Property(l => l.Id).ValueGeneratedOnAdd();
        cornCall.HasIndex(l => l.CornId);
        cornCall.HasIndex(l => l.CallId);
        cornCall.HasIndex(l => new { l.CornId, l.CallId }).IsUnique();
        cornCall.HasOne(l => l.CornEntity)
            .WithMany(c => c.CornCallLinks)
            .HasForeignKey(l => l.CornId)
            .OnDelete(DeleteBehavior.Cascade);
        cornCall.HasOne(l => l.CallEntity)
            .WithMany(c => c.CornCallLinks)
            .HasForeignKey(l => l.CallId)
            .OnDelete(DeleteBehavior.Cascade);
        cornCall.Property(l => l.MatchingPhone)
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
    }
}
