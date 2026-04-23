using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal sealed class SandwichPersistence(
    IDataPersistence<SandEntity> persist,
    IVoToEntity<Sandwich, SandEntity> voToE
    ) : VoPersistence<SandEntity, Sandwich>(persist, voToE), IDataPersistence<Sandwich>
{ }
