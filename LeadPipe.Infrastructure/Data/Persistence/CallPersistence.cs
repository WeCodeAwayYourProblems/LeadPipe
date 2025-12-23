using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal sealed class CallPersistence(
    IDataPersistence<CallEntity> persist,
    IVoToEntity<Call, CallEntity> voToE
    ) : IDataPersistence<Call>
{
    private readonly IDataPersistence<CallEntity> _persist = persist;
    private readonly IVoToEntity<Call, CallEntity> _voToE = voToE;
    public async Task<Result> SaveAsync(List<Call> t)
    {
        List<CallEntity> entities = [.. t.Select(_voToE.Translate)];
        Result result = await _persist.SaveAsync(entities);
        return result;
    }
}