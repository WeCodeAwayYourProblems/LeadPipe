using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal sealed class CustardPersistence(
    IDataPersistence<CustardEntity> persist,
    IVoToEntity<Custard, CustardEntity> voToE
    ) : VoPersistence<CustardEntity, Custard>(persist, voToE), IDataPersistence<Custard>
{ }
