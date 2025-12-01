using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;

namespace LeadPipe.Infrastructure.Translate;

public interface IVoToEntity
{
    SubsEntity Translate(Sandwich s);
    PlumbingEntity Translate(Plumbing plumbing);
}