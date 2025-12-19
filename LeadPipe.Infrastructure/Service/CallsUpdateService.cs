using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Service;

internal sealed class CallsUpdateService(
    IDataSourceAsync<CallMySqlEntity> call,
    IEntityToVo<CallMySqlEntity, Call> eToVo,
    IVoToEntity<Call, CallMySqlEntity> voToE,
    IDataPersistence<CallMySqlEntity> persistence
    ) : IUpdateService<Call>
{
    private readonly IDataSourceAsync<CallMySqlEntity> _call = call;
    private readonly IEntityToVo<CallMySqlEntity, Call> _eToVo = eToVo;
    private readonly IVoToEntity<Call, CallMySqlEntity> _voToE = voToE;
    private readonly IDataPersistence<CallMySqlEntity> _persistence = persistence;
    public async Task<Result<List<Call>>> GetDataAsync()
    {
        // Retrieve all data from source
        Result<List<CallMySqlEntity>> callsResult = await _call.LoadAsync();
        if (callsResult.IsFailure)
            return Result.Failure<List<Call>>(callsResult.Error);

        return callsResult.IsSuccess
            ? Result.Success(callsResult.Value.Select(_eToVo.Translate).ToList())
            : Result.Failure<List<Call>>(callsResult.Error);
    }

    public async Task<Result> SaveDataAsync(List<Call> data)
    {
        List<CallMySqlEntity> calls = [.. data.Select(_voToE.Translate)];
        Result saved = await _persistence.SaveAsync(calls);
        return saved;
    }

    public async Task<Result<List<Call>>> UpdateDataAsync()
    {
        var callsResult = await _call.RefreshAsync();
        if (callsResult.IsFailure)
            return Result.Failure<List<Call>>(callsResult.Error);

        return callsResult.IsSuccess
            ? Result.Success(callsResult.Value.Select(_eToVo.Translate).ToList())
            : Result.Failure<List<Call>>(callsResult.Error);
    }
}

