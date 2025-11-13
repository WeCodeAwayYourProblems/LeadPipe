using LeadPipe.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.Database;

internal class PlumbingContext : DbContext
{
    public DbSet<SubsEntity> SubsEntities { get; set; }
    public DbSet<LeafEntity> LeafEntities { get; set; }
    public DbSet<YellerEntity> YellerEntities { get; set; }
    public DbSet<CalliEntity> CalliEntities { get; set; }
    public DbSet<LabEntity> LabEntities { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SubsEntity
        EntityTypeBuilder<SubsEntity> sub = modelBuilder.Entity<SubsEntity>();
        sub.HasKey(s => s.Id);

        // Add Indexes on foreign keys
        sub.HasIndex(s => s.LeafPhoneNumber);
        sub.HasIndex(s => s.YellerPhoneNumber);
        sub.HasIndex(s => s.CalliPhoneNumber);
        sub.HasIndex(s => s.LabPhoneNumber);

        // LeafEntity
        EntityTypeBuilder<LeafEntity> leaf = modelBuilder.Entity<LeafEntity>();
        leaf.HasIndex(l => l.PhoneNumber);
        sub.HasOne(s => s.LeafEntity)
            .WithMany()
            .HasForeignKey(s => s.LeafPhoneNumber)
            .HasPrincipalKey(s => s.PhoneNumber);

        // YellerEntity
        EntityTypeBuilder<YellerEntity> yeller = modelBuilder.Entity<YellerEntity>();
        yeller.HasIndex(y => y.PhoneNumber);
        sub.HasOne(s => s.YellerEntity)
            .WithMany()
            .HasForeignKey(s => s.LeafPhoneNumber)
            .HasPrincipalKey(s => s.PhoneNumber);

        // CalliEntity
        EntityTypeBuilder<CalliEntity> calli = modelBuilder.Entity<CalliEntity>();
        calli.HasIndex(c => c.PhoneNumber);
        sub.HasOne(s => s.CalliEntity)
            .WithMany()
            .HasForeignKey(s => s.LeafPhoneNumber)
            .HasPrincipalKey(s => s.PhoneNumber);

        // LabEntity
        EntityTypeBuilder<LabEntity> lab = modelBuilder.Entity<LabEntity>();
        lab.HasIndex(l => l.PhoneNumber);
        sub.HasOne(s => s.LabEntity)
            .WithMany()
            .HasForeignKey(s => s.LeafPhoneNumber)
            .HasPrincipalKey(s => s.PhoneNumber);
    }
}
