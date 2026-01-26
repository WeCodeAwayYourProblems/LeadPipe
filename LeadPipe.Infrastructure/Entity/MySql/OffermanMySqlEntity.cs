using System.ComponentModel.DataAnnotations.Schema;

namespace LeadPipe.Infrastructure.Entity.MySql;

public class OffermanMySqlEntity
{
#pragma warning disable IDE1006 // Naming Styles
    public string? branchName { get; set; }
    public int officeID { get; set; }
    public string? iconRed { get; set; }
    public string? iconBlue { get; set; }
    public string? iconGrey { get; set; }
    public string? officeAddress { get; set; }
    public double lat { get; set; }
    [Column("long")]
    public double longitude { get; set; }
    public int active { get; set; }
    public string? branchEmaill { get; set; }
    public int fox_region_id { get; set; }
    public int district_id { get; set; }
    public int year_created { get; set; }
    public string? timeZone { get; set; }
    public string? merchantID { get; set; }
    public string? officeState { get; set; }
    public string? officeZip { get; set; }
    public string? officeCity { get; set; }
    public string? officeStreetAddress { get; set; }
    public string? phone { get; set; }
    public string? licenseNumber { get; set; }
    public int fds { get; set; }
    public DateTime? dateDeleted { get; set; }

    // Navigation property
    public ICollection<SandMySqlEntity> SandEntities { get; set; } = [];

#pragma warning restore IDE1006 // Naming Styles
}