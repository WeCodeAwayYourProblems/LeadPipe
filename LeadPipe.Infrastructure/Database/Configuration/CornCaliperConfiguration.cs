using LeadPipe.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.Database.Configuration;

internal sealed class CornCaliperConfiguration : IEntityTypeConfiguration<CornCaliperLink>
{
    public void Configure(EntityTypeBuilder<CornCaliperLink> builder)
    {
        var cornCaliper = builder
            .ToTable(TableNames.CornCaliperLinksName, t =>
            {
                t.HasCheckConstraint(
                "CK_CornCaliper_MatchingPhone",
                $"{nameof(CornCaliperLink.MatchingPhone)} <> 0");
            });
        cornCaliper.HasKey(l => l.Id);
        cornCaliper.Property(l => l.Id).ValueGeneratedOnAdd();
        cornCaliper.HasIndex(l => l.CaliperId);
        cornCaliper.HasIndex(l => l.UnixMatchDate);
        cornCaliper.HasIndex(l => new { l.CornId, l.CaliperId }).IsUnique(); // DO NOT change the order here
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
    }
}