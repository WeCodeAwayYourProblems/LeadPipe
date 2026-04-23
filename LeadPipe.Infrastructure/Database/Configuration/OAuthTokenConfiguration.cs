using LeadPipe.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPipe.Infrastructure.Database.Configuration;

internal sealed class OAuthTokenConfiguration : IEntityTypeConfiguration<OAuthTokenEntity>
{
    public void Configure(EntityTypeBuilder<OAuthTokenEntity> builder)
    {
        builder.ToTable(TableNames.OAuthTokensName);
        builder.HasKey(x => x.Provider);
        builder.Property(x => x.Provider)
            .IsRequired();
        builder.Property(x => x.AccessToken)
            .IsRequired();
        builder.Property(x => x.TokenType)
            .IsRequired();
        builder.Property(x => x.ExpiresAtUtc)
            .IsRequired();
        builder.Property(x => x.UnixExpiresAtUtc)
            .IsRequired();
        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();
        builder.Property(x => x.UnixUpdatedAtUtc)
            .IsConcurrencyToken() // Redundant but safe
            .IsRequired();
    }
}