using LeadPipe.Infrastructure.Entity.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.Database.Configuration;

internal sealed class CustardPlumbingConfiguration : IEntityTypeConfiguration<CustardPlumbingLink>
{
    public void Configure(EntityTypeBuilder<CustardPlumbingLink> builder)
    {
        builder.ToTable(TableNames.CustardPlumbingLinksName, t =>
        {
            t.HasCheckConstraint(
                "CK_CustardPlumbing_MatchingPhone",
                $"{nameof(CustardPlumbingLink.MatchingPhone)} <> 0");
        });
        builder.HasKey(l => l.Id);
        builder.HasIndex(l => l.PlumbingId);
        builder.HasIndex(l => l.UnixMatchDate);
        builder.Property(l => l.Id)
            .ValueGeneratedOnAdd();
        builder.HasIndex(l => new { l.CustardId, l.PlumbingId }).IsUnique(); // Order Matters
        builder.Property(l => l.CustardId).IsRequired();
        builder.Property(l => l.PlumbingId).IsRequired();
        builder.HasOne(l => l.Custard)
          .WithMany(c => c.CustardPlumbingLinks)
          .HasForeignKey(l => l.CustardId)
          .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(l => l.Plumbing)
          .WithMany(c => c.CustardPlumbingLinks)
          .HasForeignKey(l => l.PlumbingId)
          .OnDelete(DeleteBehavior.Cascade);
        builder.Property(l => l.MatchingPhone).IsRequired();
    }
}