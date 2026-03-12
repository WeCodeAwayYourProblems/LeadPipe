using LeadPipe.Infrastructure.Entity.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.Sqlite.Context.Configuration;

internal sealed class SandPlumbingConfiguration : IEntityTypeConfiguration<SandPlumbingLink>
{
    public void Configure(EntityTypeBuilder<SandPlumbingLink> builder)
    {
        var spLink = builder
            .ToTable(TableNames.SandPlumbingLinksName, t =>
            {
                t.HasCheckConstraint(
                    "CK_SandPlumbing_MatchingPhone",
                    $"{nameof(SandPlumbingLink.MatchingPhone)} <> 0");
            });
        spLink.HasKey(l => l.Id);
        spLink.Property(l => l.Id).ValueGeneratedOnAdd();
        spLink.HasIndex(l => l.PlumbingId);
        spLink.HasIndex(l => l.UnixMatchDate);
        spLink.HasIndex(l => new { l.SandId, l.PlumbingId }).IsUnique(); // Order Matter
        spLink.HasOne(l => l.SandEntity)
            .WithMany(s => s.SandPlumbingLinks)
            .HasForeignKey(l => l.SandId)
            .OnDelete(DeleteBehavior.Cascade);
        spLink.HasOne(l => l.PlumbingEntity)
            .WithMany(p => p.SandPlumbingLinks)
            .HasForeignKey(l => l.PlumbingId)
            .OnDelete(DeleteBehavior.Cascade);
        spLink.Property(l => l.MatchingPhone).IsRequired();
    }
}