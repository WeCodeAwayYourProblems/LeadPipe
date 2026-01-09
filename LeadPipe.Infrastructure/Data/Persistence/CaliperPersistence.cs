using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal sealed class CaliperPersistence(
    IDataPersistence<CaliperEntity> persist,
    IVoToEntity<Caliper, CaliperEntity> voToE
    ) : IDataPersistence<Caliper>
{
    private readonly IDataPersistence<CaliperEntity> _persist = persist;
    private readonly IVoToEntity<Caliper, CaliperEntity> _voToE = voToE;
    public async Task<Result> SaveAsync(List<Caliper> t)
    {
        List<CaliperEntity> entities = [.. t.Select(_voToE.Translate)];
        Result result = await _persist.SaveAsync(entities);
        return result;
    }
}