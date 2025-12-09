using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Manager;

public interface IUpdateManager
{
    Task<Result<List<Plumbing>>> ManageAsync(bool update = true);
}
public abstract class UpdateManager(IUpdateService<Plumbing> update)
{
    private readonly IUpdateService<Plumbing> _update = update;
    public async Task<Result<List<Plumbing>>> ManageAsync(bool update = true)
    {
        Result<List<Plumbing>> data = update
            ? await _update.UpdateDataAsync()
            : await _update.GetDataAsync();

        if (data.IsFailure)
            return data;

        Result saved = await _update.SaveDataAsync(data.Value);
        Result<List<Plumbing>> result = saved.IsSuccess
            ? data.Value
            : Result.Failure<List<Plumbing>>(saved.Error);

        return result;
    }
}
