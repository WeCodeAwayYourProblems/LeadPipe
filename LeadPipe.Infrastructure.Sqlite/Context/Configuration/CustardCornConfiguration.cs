using LeadPipe.Infrastructure.Entity.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.Sqlite.Context.Configuration;

internal sealed class CustardCornConfiguration : IEntityTypeConfiguration<CustardCornLink>
{
    public void Configure(EntityTypeBuilder<CustardCornLink> builder)
    {
        builder.ToTable(TableNames.CustardCornLinksName, t =>
        {
            t.HasCheckConstraint(
                "CK_CustardCorn_MatchingPhone",
                $"{nameof(CustardCornLink.MatchingPhone)} <> 0");
        });
        builder.HasKey(l => l.Id);
        builder.HasIndex(l => l.CornId);
        builder.HasIndex(l => l.UnixMatchDate);
        builder.Property(l => l.Id)
            .ValueGeneratedOnAdd();
        builder.HasIndex(l => new { l.CustardId, l.CornId }).IsUnique();
        builder.Property(l => l.CustardId).IsRequired();
        builder.Property(l => l.CornId).IsRequired();
        builder.HasOne(l => l.Custard)
          .WithMany(c => c.CustardCornLinks)
          .HasForeignKey(l => l.CustardId)
          .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(l => l.Corn)
          .WithMany(c => c.CustardCornLinks)
          .HasForeignKey(l => l.CornId)
          .OnDelete(DeleteBehavior.Cascade);
        builder.Property(l => l.MatchingPhone).IsRequired();
    }
}