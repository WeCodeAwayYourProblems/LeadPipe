using LeadPipe.Infrastructure.Entity.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.Sqlite.Context;

public class PlumbingContext(DbContextOptions<PlumbingContext> options) : DbContext(options)
{
    public DbSet<SubsEntity> SubsEntities { get; set; }
    public DbSet<PlumbingEntity> PlumbingEntities { get; set; }
    public DbSet<SubsPlumbingLink> SubsPlumbingLinks { get; set; }
    public DbSet<CallEntity> CallEntities { get; set; }
    public DbSet<SubsCallLink> SubsCallLinks { get; set; }
    public DbSet<PlumbingCallLink> PlumbingCallLinks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SubsEntity
        var sub = modelBuilder.Entity<SubsEntity>();
        sub.HasKey(s => s.Id);
        sub.HasIndex(s => s.Number);
        sub.HasIndex(s => s.Number2);

        // PlumbingEntity
        var plumb = modelBuilder.Entity<PlumbingEntity>();
        plumb.HasKey(p => p.Id);
        plumb.Property(p => p.Id).ValueGeneratedOnAdd();
        plumb.HasIndex(p => p.PhoneNumber);
        plumb.Property(p => p.Source).HasConversion<string>();

        // CallEntity
        var call = modelBuilder.Entity<CallEntity>();
        call.HasKey(c => c.Id);
        call.HasIndex(c => c.PhoneNumber);
        call.Property(c => c.CallDate).IsRequired();
        call.Property(c => c.UnixCallDate).IsRequired();

        // SubsPlumbingLink
        var spLink = modelBuilder.Entity<SubsPlumbingLink>();
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
        var subsCall = modelBuilder.Entity<SubsCallLink>();
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
        var plumbCall = modelBuilder.Entity<PlumbingCallLink>();
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
    }
}