using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal sealed class SandwichPersistence(
    IDataPersistence<SubsEntity> persist,
    IVoToEntity<Sandwich, SubsEntity> voToE
    ) : IDataPersistence<Sandwich>
{
    private readonly IDataPersistence<SubsEntity> _persist = persist;
    private readonly IVoToEntity<Sandwich, SubsEntity> _voToE = voToE;
    public async Task<Result> SaveAsync(List<Sandwich> t)
    {
        List<SubsEntity> entities = [.. t.Select(_voToE.Translate)];
        Result result = await _persist.SaveAsync(entities);
        return result;
    }
}