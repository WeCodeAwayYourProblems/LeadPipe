using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Interfaces.Core;
using System.Diagnostics.CodeAnalysis;

namespace LeadPipe.Infrastructure.Entity;

public class CornEntity : IEntity, IPhoneDateIdEntity
{
    public CornEntity() { }
    [SetsRequiredMembers]
    private CornEntity(CornEntity c)
    {
        Id = c.Id;
        PhoneNumber = c.PhoneNumber;
        Date = c.Date;
        UnixDate = c.UnixDate;
        Payload = c.Payload;
        MetaData = c.MetaData;
        Source = c.Source;
        UtmSource = c.UtmSource;
        UtmMedium = c.UtmMedium;
        UtmCampaign = c.UtmCampaign;
        UtmContent = c.UtmContent;
        UtmTerm = c.UtmTerm;

        CustardCornLinks = [.. c.CustardCornLinks.Select(c => c)];
        SandCornLinks = [.. c.SandCornLinks.Select(c => c)];
        CornCaliperLinks = [.. c.CornCaliperLinks.Select(c => c)];
        CornPlumbingLinks = [.. c.CornPlumbingLinks.Select(c => c)];
    }
    public required long Id { get; set; }
    public required PhoneNumber PhoneNumber { get; set; }
    public required DateTime Date { get; set; }
    public required long UnixDate { get; set; }
    public required string Payload { get; set; }
    public required string MetaData { get; set; }
    public required string Source { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmContent { get; set; }
    public string? UtmTerm { get; set; }

    public ICollection<CustardCornLink> CustardCornLinks { get; set; } = [];
    public ICollection<SandCornLink> SandCornLinks { get; set; } = [];
    public ICollection<CornCaliperLink> CornCaliperLinks { get; set; } = [];
    public ICollection<CornPlumbingLink> CornPlumbingLinks { get; set; } = [];

    internal CornEntity Clone() => new(this);
}
