using LeadPipe.Infrastructure.Entity.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.Sqlite.Context.Configuration;

internal sealed class CustardCaliperConfiguration : IEntityTypeConfiguration<CustardCaliperLink>
{
    public void Configure(EntityTypeBuilder<CustardCaliperLink> builder)
    {
        builder.ToTable(TableNames.CustardCaliperLinksName, t =>
        {
            t.HasCheckConstraint(
                "CK_CustardCaliper_MatchingPhone",
                $"{nameof(CustardCaliperLink.MatchingPhone)} <> 0");
        });
        builder.HasKey(l => l.Id);
        builder.HasIndex(l => l.CaliperId);
        builder.HasIndex(l => l.UnixMatchDate);
        builder.Property(l => l.Id)
            .ValueGeneratedOnAdd();
        builder.HasIndex(l => new { l.CustardId, l.CaliperId }).IsUnique(); // Order matters here
        builder.Property(l => l.CustardId).IsRequired();
        builder.Property(l => l.CaliperId).IsRequired();
        builder.HasOne(l => l.Custard)
          .WithMany(c => c.CustardCaliperLinks)
          .HasForeignKey(l => l.CustardId)
          .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(l => l.Caliper)
          .WithMany(c => c.CustardCaliperLinks)
          .HasForeignKey(l => l.CaliperId)
          .OnDelete(DeleteBehavior.Cascade);
        builder.Property(l => l.MatchingPhone).IsRequired();
    }
}