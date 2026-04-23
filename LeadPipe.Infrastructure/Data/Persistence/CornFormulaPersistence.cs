using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal sealed class CornFormulaPersistence(
    IDataPersistence<CornEntity> persist,
    IVoToEntity<CornFormula, CornEntity> voToE
) : VoPersistence<CornEntity, CornFormula>(persist, voToE), IDataPersistence<CornFormula>
{ }