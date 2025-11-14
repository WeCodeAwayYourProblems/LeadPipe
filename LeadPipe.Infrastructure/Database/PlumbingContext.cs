using LeadPipe.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.Database;

internal class PlumbingContext(DbContextOptions<PlumbingContext> options) : DbContext(options)
{
    public DbSet<SubsEntity> SubsEntities { get; set; }
    public DbSet<PlumbingEntity> PlumbingEntities { get; set; }
    public DbSet<SubsPlumbingLink> SubsPlumbingLinks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SubsEntity configuration
        var sub = modelBuilder.Entity<SubsEntity>();
        sub.HasKey(s => s.Id);
        sub.HasIndex(s => s.Number);
        sub.HasIndex(s => s.Number2);

        // PlumbingEntity configuration
        var plumb = modelBuilder.Entity<PlumbingEntity>();
        plumb.HasKey(p => p.Id);
        plumb.Property(p => p.Id)
            .ValueGeneratedOnAdd();
        plumb.HasIndex(p => p.PhoneNumber);
        plumb.Property(p => p.Source)
             .HasConversion<string>();

        // SubsPlumbingLink configuration (join table)
        var link = modelBuilder.Entity<SubsPlumbingLink>();
        link.HasKey(l => new { l.SubsId, l.PlumbingId });
        link.HasIndex(l => l.SubsId);
        link.HasIndex(l => l.PlumbingId);

        link.HasOne(l => l.SubsEntity)
            .WithMany(s => s.SubsPlumbingLinks)
            .HasForeignKey(l => l.SubsId)
            .OnDelete(DeleteBehavior.Cascade);

        link.HasOne(l => l.PlumbingEntity)
            .WithMany(p => p.SubsPlumbingLinks)
            .HasForeignKey(l => l.PlumbingId)
            .OnDelete(DeleteBehavior.Cascade);

        link.Property(l => l.MatchingSubPhone)
            .IsRequired();
    }
}