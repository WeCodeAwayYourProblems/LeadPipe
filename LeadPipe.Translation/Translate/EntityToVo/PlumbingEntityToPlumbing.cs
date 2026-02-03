using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.EntityToVo;

internal class PlumbingEntityToPlumbing : IEntityToVo<PlumbingEntity, Plumbing>
{
    public Plumbing Translate(PlumbingEntity entity)
    {
        var number = new PhoneNumber(entity.PhoneNumber);
        DateTime d = DateTime.SpecifyKind(entity.Date, DateTimeKind.Utc);
        DateTimeOffset date = new(d, TimeSpan.Zero);
        var contents = entity.Contents;
        var source = entity.Source;

        Plumbing result = new
        (
            entity.Id,
            PhoneNumber: number,
            Date: date,
            Contents: contents,
            Branch: entity.Branch,
            MetaData: entity.MetaData,
            Source: source
        );
        return result;
    }
}
