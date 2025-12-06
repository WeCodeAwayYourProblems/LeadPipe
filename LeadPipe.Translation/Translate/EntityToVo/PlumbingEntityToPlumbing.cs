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
        DateTimeOffset date = new(d, TimeSpan.FromSeconds(0));
        var contents = entity.Contents;
        var source = entity.Source;

        var result = new Plumbing(PhoneNumber: number, Date: date, Contents: contents, Source: source);
        return result;
    }
}
