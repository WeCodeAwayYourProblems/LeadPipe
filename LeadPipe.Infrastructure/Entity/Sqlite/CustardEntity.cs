using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Interfaces.Core;
using System.Diagnostics.CodeAnalysis;

namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class CustardEntity : IEntity, IPhoneDateIdEntity
{
    [SetsRequiredMembers]
    private CustardEntity(CustardEntity entity)
    {
        Id = entity.Id;
        Active = entity.Active;
        PhoneNumber = entity.PhoneNumber;
        PhoneNumber2 = entity.PhoneNumber2;
        Date = entity.Date;
        UnixDate = entity.UnixDate;
        CancelDate = entity.CancelDate;
        UnixCancelDate = entity.UnixCancelDate;
        SandEntities = [.. entity.SandEntities.Select(s => s.Clone())];
        CustardCaliperLinks = [.. entity.CustardCaliperLinks.Select(c => c.Clone())];
        CustardCornLinks = [.. entity.CustardCornLinks.Select(c => c.Clone())];
        CustardPlumbingLinks = [.. entity.CustardPlumbingLinks.Select(c => c.Clone())];

    }
    public CustardEntity() { }
    public CustardEntity Clone() => new(this);

    public required long Id { get; set; }
    public bool Active { get; set; }
    public required PhoneNumber PhoneNumber { get; set; }
    public PhoneNumber? PhoneNumber2 { get; set; }
    public DateTime Date { get; set; }
    public required long UnixDate { get; set; }
    public DateTime CancelDate { get; set; }
    public long UnixCancelDate { get; set; }

    // Navigation    
    public ICollection<SandEntity> SandEntities { get; set; } = [];
    public ICollection<CustardCaliperLink> CustardCaliperLinks { get; set; } = [];
    public ICollection<CustardCornLink> CustardCornLinks { get; set; } = [];
    public ICollection<CustardPlumbingLink> CustardPlumbingLinks { get; set; } = [];
}
