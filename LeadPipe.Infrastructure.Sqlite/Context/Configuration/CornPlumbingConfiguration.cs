using LeadPipe.Infrastructure.Entity.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.Sqlite.Context.Configuration;

internal sealed class CornPlumbingConfiguration : IEntityTypeConfiguration<CornPlumbingLink>
{
    public void Configure(EntityTypeBuilder<CornPlumbingLink> builder)
    {
        var cornPlumb = builder
            .ToTable(TableNames.CornPlumbingLinksName, t =>
            {
                t.HasCheckConstraint(
                    "CK_CornPlumbing_MatchingPhone",
                    $"{nameof(CornPlumbingLink.MatchingPhone)} <> 0");
            });
        cornPlumb.HasKey(l => l.Id);
        cornPlumb.Property(l => l.Id).ValueGeneratedOnAdd();
        cornPlumb.HasIndex(l => l.PlumbingId);
        cornPlumb.HasIndex(l => l.UnixMatchDate);
        cornPlumb.HasIndex(l => new { l.CornId, l.PlumbingId }).IsUnique(); // DO NOT change the order here
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