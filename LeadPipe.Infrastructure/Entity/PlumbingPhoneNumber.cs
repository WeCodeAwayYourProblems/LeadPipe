using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Interfaces.Core;
using System.Diagnostics.CodeAnalysis;

namespace LeadPipe.Infrastructure.Entity;

public class PlumbingPhoneNumber : IEntity
{
    public PlumbingPhoneNumber() { }
    [SetsRequiredMembers]
    private PlumbingPhoneNumber(PlumbingPhoneNumber p)
    {
        Id = p.Id;
        PhoneNumber = p.PhoneNumber;
        PlumbingId = p.PlumbingId;
        Plumbing = p.Plumbing;
    }

    public long Id { get; set; }
    public required PhoneNumber PhoneNumber { get; set; }
    public required long PlumbingId { get; set; }
    public PlumbingEntity? Plumbing { get; set; }

    internal PlumbingPhoneNumber Clone() => new(this);
}