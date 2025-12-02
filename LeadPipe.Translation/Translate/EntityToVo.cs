using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Translate;
using LeadPipe.Translation.Primitives;

namespace LeadPipe.Translation.Translate;

internal class EntityToVo(IDateTimeTranslate dtranslate) : IEntityToVo
{
    private readonly IDateTimeTranslate _dt = dtranslate;
    public Plumbing Translate(PlumbingEntity entity)
    {
        var number = new PhoneNumber(entity.PhoneNumber);
        DateTimeOffset date = new(entity.Date, TimeSpan.FromSeconds(0));
        var contents = entity.Contents;
        var source = entity.Source;

        var result = new Plumbing(PhoneNumber: number, Date: date, Contents: contents, Source: source);
        return result;
    }

    public Sandwich Translate(SubsEntity entity)
    {
        var result = new Sandwich()
        {
            SubscriptionId = entity.Id,
            CustomerId = entity.CustomerId,
            Date = _dt.Convert(entity.Date, ETimeZone.Pacific),
            SubDate = _dt.Convert(entity.SubDate, ETimeZone.Pacific),
            Number = new(entity.Number),
            Number2 = new(entity.Number2),
            CancelDate = _dt.Convert(entity.CancelDate, ETimeZone.Pacific),
            SubCancelDate = _dt.Convert(entity.SubCancelDate, ETimeZone.Pacific),
            Active = entity.Active,
            SubActive = entity.SubActive,
            Complete = entity.Complete,
            Value = entity.Value,
            Seller = entity.Seller,
            Seller2 = entity.Seller2,
            Seller3 = entity.Seller3
        };
        return result;
    }

    public Call Translate(CallEntity c)
    {
        DateTimeOffset date = new(c.CallDate, TimeSpan.FromSeconds(0));
        PhoneNumber number = new(c.PhoneNumber);
        TimeSpan duration = TimeSpan.FromSeconds(c.Duration);
        string note = c.Note;
        string source = c.Source;
        bool billable = c.Billable;

        var result = new Call(Date: date, Number: number, Duration: duration, Note: note, Source: source, Billable: billable);
        return result;
    }
}
