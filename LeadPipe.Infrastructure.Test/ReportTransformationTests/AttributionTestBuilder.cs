using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;

namespace LeadPipe.Infrastructure.Test.ReportTransformationTests;

internal sealed class AttributionTestBuilder
{
#pragma warning disable CA1822 // Mark members as static
    public PhoneNumber Phone { get; } = new PhoneNumber("5551112222");

    public CustardEntity Custard(long id, long unixDate)
    {
        return new CustardEntity
        {
            Id = id,
            Active = true,
            PhoneNumber = Phone,
            UnixDate = unixDate,
            SandEntities = [],
            CustardPlumbingLinks = [],
            CustardCornLinks = [],
            CustardCaliperLinks = []
        };
    }

    public PlumbingEntity Plumbing(long id, long unixDate)
    {
        return new PlumbingEntity
        {
            Id = id,
            PhoneNumber = Phone,
            UnixDate = unixDate,
            MetaData = "x",
            Source = Source.Yeller
        };
    }

    public SandEntity Sand(CustardEntity custard, long id, long unixDate, bool complete = true, decimal value = 0)
    {
        var sand = new SandEntity
        {
            Id = id,
            CustardId = custard.Id,
            CustardEntity = custard,
            UnixDate = unixDate,
            Complete = complete,
            Active = true,
            Offerman = "bob",
            Value = value
        };

        custard.SandEntities.Add(sand);
        return sand;
    }

    public CustardPlumbingLink PlumbingLink(CustardEntity c, PlumbingEntity p, long id = 1)
    {
        var link = new CustardPlumbingLink
        {
            Id = id,
            CustardId = c.Id,
            Custard = c,
            PlumbingId = p.Id,
            Plumbing = p,
            MatchingPhone = Phone.Number,
            UnixMatchDate = c.UnixDate
        };

        c.CustardPlumbingLinks.Add(link);
        return link;
    }

    public CustardCornLink CornLink(CustardEntity c, long matchDate, out CornEntity corn, long id = 1)
    {
        corn = new CornEntity
        {
            Id = id,
            PhoneNumber = Phone,
            UnixDate = matchDate,
            Date = DateTime.UtcNow,
            Payload = "x",
            MetaData = "x",
            Source = "x",
            ReferralSource = "x",
        };

        var link = new CustardCornLink
        {
            Id = id,
            CustardId = c.Id,
            Custard = c,
            CornId = corn.Id,
            Corn = corn,
            MatchingPhone = Phone.Number,
            UnixMatchDate = matchDate
        };

        c.CustardCornLinks.Add(link);
        return link;
    }

    public CustardCaliperLink CaliperLink(CustardEntity c, long matchDate, out CaliperEntity caliper, long id = 1)
    {
        caliper = new CaliperEntity
        {
            Id = id,
            PhoneNumber = Phone,
            UnixDate = matchDate,
            Note = "x",
            Source = "x",
            Label = "x",
            Location = "x"
        };

        var link = new CustardCaliperLink
        {
            Id = id,
            CustardId = c.Id,
            Custard = c,
            CaliperId = caliper.Id,
            Caliper = caliper,
            MatchingPhone = Phone.Number,
            UnixMatchDate = matchDate
        };

        c.CustardCaliperLinks.Add(link);
        return link;
    }
#pragma warning restore CA1822 // Mark members as static
}