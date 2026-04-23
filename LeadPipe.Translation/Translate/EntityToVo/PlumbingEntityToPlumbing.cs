using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.EntityToVo;

internal class PlumbingEntityToPlumbing : IEntityToVo<PlumbingEntity, Plumbing>
{
    public Plumbing Translate(PlumbingEntity entity)
    {
        var number = new PhoneNumber(entity.PhoneNumber);
        DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(entity.UnixDate);
        var contents = entity.Contents;
        var source = entity.Source;
        PhoneNumber[] numbers = [.. entity.PhoneNumbers.Select(p => p.PhoneNumber)];

        Plumbing result = new
        (
            entity.Id,
            PhoneNumber: number,
            Date: date,
            Contents: contents,
            Branch: entity.Branch,
            MetaData: entity.MetaData,
            Source: source,
            Numbers: numbers
        );
        return result;
    }
}
