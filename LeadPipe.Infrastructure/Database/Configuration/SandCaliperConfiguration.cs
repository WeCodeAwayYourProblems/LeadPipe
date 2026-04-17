using LeadPipe.Infrastructure.Entity.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.Database.Configuration;

internal sealed class SandCaliperConfiguration : IEntityTypeConfiguration<SandCaliperLink>
{
    public void Configure(EntityTypeBuilder<SandCaliperLink> builder)
    {
        var sandCaliper = builder
            .ToTable(TableNames.SandCaliperLinksName, t =>
            {
                t.HasCheckConstraint(
                    "CK_SandCaliper_MatchingPhone",
                    $"{nameof(SandCaliperLink.MatchingPhone)}");
            });
        sandCaliper.HasKey(sc => sc.Id);
        sandCaliper.Property(sc => sc.Id).ValueGeneratedOnAdd();
        sandCaliper.HasIndex(sc => sc.CaliperId);
        sandCaliper.HasIndex(sc => sc.UnixMatchDate);
        sandCaliper.HasIndex(l => new { l.SandId, l.CaliperId }).IsUnique(); // Order matters
        sandCaliper.HasOne(sc => sc.SandEntity)
            .WithMany(s => s.SandCaliperLinks)
            .HasForeignKey(sc => sc.SandId)
            .OnDelete(DeleteBehavior.Cascade);
        sandCaliper.HasOne(sc => sc.CaliperEntity)
            .WithMany(c => c.SandCaliperLinks)
            .HasForeignKey(sc => sc.CaliperId)
            .OnDelete(DeleteBehavior.Cascade);
        sandCaliper.Property(sc => sc.MatchingPhone).IsRequired();
    }
}