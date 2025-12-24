using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;

namespace LeadPipe.Application.UpdateReportPipeline;

internal interface IUpdateManager<TVo>
{
    Task<Result<List<TVo>>> ManageAsync(bool update = true);
}
internal abstract class UpdateManager<TVo>(IUpdateService<TVo> update)
{
    private readonly IUpdateService<TVo> _update = update;
    public async Task<Result<List<TVo>>> ManageAsync(bool update = true)
    {
        Result<List<TVo>> data = update
            ? await _update.UpdateDataAsync()
            : await _update.GetDataAsync();

        if (data.IsFailure)
            return data;

        Result saved = await _update.SaveDataAsync(data.Value);
        Result<List<TVo>> result = saved.IsSuccess
            ? data.Value
            : Result.Failure<List<TVo>>(saved.Error);

        return result;
    }
}
