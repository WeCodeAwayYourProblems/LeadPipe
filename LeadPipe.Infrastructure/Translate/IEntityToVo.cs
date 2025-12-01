using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;

namespace LeadPipe.Infrastructure.Translate;

public interface IEntityToVo
{
    Plumbing Translate(PlumbingEntity entity);
    Sandwich Translate(SubsEntity entity);
}