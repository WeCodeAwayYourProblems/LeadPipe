using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Interfaces.Core;
using System.Diagnostics.CodeAnalysis;

namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class SandEntity : IEntity
{
    public SandEntity() { }
    [SetsRequiredMembers]
    private SandEntity(SandEntity s)
    {
        Id = s.Id;
        CustardId = s.CustardId;
        Date = s.Date;
        UnixDate = s.UnixDate;
        CancelDate = s.CancelDate;
        UnixCancelDate = s.UnixCancelDate;
        Active = s.Active;
        Complete = s.Complete;
        Value = s.Value;
        Type = s.Type;
        Seller = s.Seller;
        Seller2 = s.Seller2;
        Seller3 = s.Seller3;
        Offerman = s.Offerman;

        CustardEntity = s.CustardEntity?.Clone();
        SandPlumbingLinks = [.. s.SandPlumbingLinks.Select(s => s)];
        SandCaliperLinks = [.. s.SandCaliperLinks.Select(s => s)];
        SandCornLinks = [.. s.SandCornLinks.Select(s => s)];

    }
    public required long Id { get; set; }
    public required long CustardId { get; set; }
    public DateTime Date { get; set; }
    public long UnixDate { get; set; }
    public DateTime CancelDate { get; set; }
    public long UnixCancelDate { get; set; }
    public bool Active { get; set; }
    public bool Complete { get; set; }
    public decimal Value { get; set; }
    public string? Type { get; set; }
    public int Seller { get; set; }
    public int Seller2 { get; set; }
    public int Seller3 { get; set; }
    public required string Offerman { get; set; }

    // Navigation
    public CustardEntity? CustardEntity { get; set; }
    public ICollection<SandPlumbingLink> SandPlumbingLinks { get; set; } = [];
    public ICollection<SandCaliperLink> SandCaliperLinks { get; set; } = [];
    public ICollection<SandCornLink> SandCornLinks { get; set; } = [];

    internal SandEntity Clone() => new(this);
}
