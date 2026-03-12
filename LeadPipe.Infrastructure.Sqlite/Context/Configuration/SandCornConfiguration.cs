using LeadPipe.Infrastructure.Entity.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.Sqlite.Context.Configuration;

internal sealed class SandCornConfiguration : IEntityTypeConfiguration<SandCornLink>
{
    public void Configure(EntityTypeBuilder<SandCornLink> builder)
    {
        var sandCorn = builder
            .ToTable(TableNames.SandCornLinksName, t =>
            {
                t.HasCheckConstraint(
                    "CK_SandCorn_MatchingPhone",
                    $"{nameof(SandCornLink.MatchingPhone)} <> 0");
            });
        sandCorn.HasKey(l => l.Id);
        sandCorn.Property(l => l.Id).ValueGeneratedOnAdd();
        sandCorn.HasIndex(l => l.CornId);
        sandCorn.HasIndex(l => l.UnixMatchDate);
        sandCorn.HasIndex(l => new { l.SandId, l.CornId }).IsUnique(); // Order Matters
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
    }
}