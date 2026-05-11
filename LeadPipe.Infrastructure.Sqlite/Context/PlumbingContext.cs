using LeadPipe.Infrastructure.Database.Configuration;
using LeadPipe.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.Sqlite.Context;

public sealed class PlumbingContext(DbContextOptions<PlumbingContext> options) : DbContext(options)
{
#pragma warning disable IDE0079
    // Entities
    public DbSet<OAuthTokenEntity> OAuthTokens => Set<OAuthTokenEntity>();
    public DbSet<SyncStateEntity> SyncState => Set<SyncStateEntity>();
    public DbSet<SyncStampEntity> SyncStamp => Set<SyncStampEntity>();
    public DbSet<CaliperEntity> CaliperEntities => Set<CaliperEntity>();
    public DbSet<CornEntity> CornEntities => Set<CornEntity>();
    public DbSet<PlumbingEntity> PlumbingEntities => Set<PlumbingEntity>();
    public DbSet<PlumbingPhoneNumber> PlumbingPhoneNumbers => Set<PlumbingPhoneNumber>();
    public DbSet<CustardEntity> CustardEntities => Set<CustardEntity>();
    public DbSet<SandEntity> SandEntities => Set<SandEntity>();

    // Links
    public DbSet<CornCaliperLink> CornCaliperLinks => Set<CornCaliperLink>();
    public DbSet<CornPlumbingLink> CornPlumbingLinks => Set<CornPlumbingLink>();
    public DbSet<PlumbingCaliperLink> PlumbingCaliperLinks => Set<PlumbingCaliperLink>();
    public DbSet<CustardCaliperLink> CustardCaliperLinks => Set<CustardCaliperLink>();
    public DbSet<CustardCornLink> CustardCornLinks => Set<CustardCornLink>();
    public DbSet<CustardPlumbingLink> CustardPlumbingLinks => Set<CustardPlumbingLink>();
    public DbSet<SandCaliperLink> SandCaliperLinks => Set<SandCaliperLink>();
    public DbSet<SandCornLink> SandCornLinks => Set<SandCornLink>();
    public DbSet<SandPlumbingLink> SandPlumbingLinks => Set<SandPlumbingLink>();

#pragma warning restore IDE0079

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConfigurationAssemblyMarker).Assembly);
    }

}
