using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Primitives;

internal class PlumbingMetaDataCanonicalPersistenceFormat : IPlumbingMetaDataCanonicalPersistenceFormat<PlumbingEntity, string>
{
    public string Translate(PlumbingEntity t) => t.MetaData is null ? string.Empty : t.MetaData;
}
