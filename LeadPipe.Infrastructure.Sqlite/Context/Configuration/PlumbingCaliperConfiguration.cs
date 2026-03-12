using LeadPipe.Infrastructure.Entity.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.Sqlite.Context.Configuration;

internal sealed class PlumbingCaliperConfiguration : IEntityTypeConfiguration<PlumbingCaliperLink>
{
    public void Configure(EntityTypeBuilder<PlumbingCaliperLink> builder)
    {
        var plumbCaliper = builder
            .ToTable(TableNames.PlumbingCaliperLinksName, t =>
            {
                t.HasCheckConstraint(
                    "CK_PlumbingCaliper_MatchingPhone",
                    $"{nameof(PlumbingCaliperLink.MatchingPhone)} <> 0");
            });
        plumbCaliper.HasKey(pc => pc.Id);
        plumbCaliper.Property(pc => pc.Id).ValueGeneratedOnAdd();
        plumbCaliper.HasIndex(pc => pc.CaliperId);
        plumbCaliper.HasIndex(pc => pc.UnixMatchDate);
        plumbCaliper.HasIndex(l => new { l.PlumbingId, l.CaliperId }).IsUnique(); // Order matters here
        plumbCaliper.HasOne(pc => pc.PlumbingEntity)
            .WithMany(p => p.PlumbingCaliperLinks)
            .HasForeignKey(pc => pc.PlumbingId)
            .OnDelete(DeleteBehavior.Cascade);
        plumbCaliper.HasOne(pc => pc.CaliperEntity)
            .WithMany(c => c.PlumbingCaliperLinks)
            .HasForeignKey(pc => pc.CaliperId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}