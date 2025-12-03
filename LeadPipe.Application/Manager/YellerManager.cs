using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.Manager;

public interface IYellerManager
{
    Task<Result<List<Plumbing>>> ManageAsync(bool update = true);
}

public class YellerManager([FromKeyedServices(Source.Yeller)] IUpdateService<Plumbing> update) : IYellerManager
{
    private readonly IUpdateService<Plumbing> _update = update;
    public async Task<Result<List<Plumbing>>> ManageAsync(bool update = true)
    {
        Result<List<Plumbing>> yellerResult = update
            ? await _update.UpdateDataAsync()
            : await _update.GetDataAsync();

        if (yellerResult.IsFailure)
            return yellerResult;

        Result saved = await _update.SaveDataAsync(yellerResult.Value);
        Result<List<Plumbing>> result = saved.IsSuccess
            ? yellerResult.Value
            : Result.Failure<List<Plumbing>>(saved.Error);

        return result;
    }
}