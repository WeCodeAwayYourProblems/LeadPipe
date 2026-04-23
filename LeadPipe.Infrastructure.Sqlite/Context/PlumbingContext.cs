using LeadPipe.Infrastructure.Database.Configuration;
using LeadPipe.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.Sqlite.Context;

public sealed class PlumbingContext(DbContextOptions<PlumbingContext> options) : DbContext(options)
{
#pragma warning disable IDE0079
    // Entities
    public DbSet<OAuthTokenEntity> OAuthTokens { get; set; }
    public DbSet<SyncStateEntity> SyncState { get; set; }
    public DbSet<SyncStampEntity> SyncStamp { get; set; }
    public DbSet<CaliperEntity> CaliperEntities { get; set; }
    public DbSet<CornEntity> CornEntities { get; set; }
    public DbSet<PlumbingEntity> PlumbingEntities { get; set; }
    public DbSet<PlumbingPhoneNumber> PlumbingPhoneNumbers { get; set; }
    public DbSet<CustardEntity> CustardEntities { get; set; }
    public DbSet<SandEntity> SandEntities { get; set; }

    // Links
    public DbSet<CornCaliperLink> CornCaliperLinks { get; set; }
    public DbSet<CornPlumbingLink> CornPlumbingLinks { get; set; }
    public DbSet<PlumbingCaliperLink> PlumbingCaliperLinks { get; set; }
    public DbSet<CustardCaliperLink> CustardCaliperLinks { get; set; }
    public DbSet<CustardCornLink> CustardCornLinks { get; set; }
    public DbSet<CustardPlumbingLink> CustardPlumbingLinks { get; set; }
    public DbSet<SandCaliperLink> SandCaliperLinks { get; set; }
    public DbSet<SandCornLink> SandCornLinks { get; set; }
    public DbSet<SandPlumbingLink> SandPlumbingLinks { get; set; }

#pragma warning restore IDE0079

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConfigurationAssemblyMarker).Assembly);
    }

}
