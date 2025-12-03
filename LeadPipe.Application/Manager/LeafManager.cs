using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.Manager;

public interface ILeafManager
{
    Task<Result<List<Plumbing>>> ManageAsync(bool update = true);
}

public class LeafManager([FromKeyedServices(Source.Leaf)] IUpdateService<Plumbing> update) : ILeafManager
{
    private readonly IUpdateService<Plumbing> _update = update;
    public async Task<Result<List<Plumbing>>> ManageAsync(bool update = true)
    {
        var plumbing = update
            ? await _update.UpdateDataAsync()
            : await _update.GetDataAsync();

        if (plumbing.IsFailure)
            return Result.Failure<List<Plumbing>>(plumbing.Error);

        Result saved = await _update.SaveDataAsync(plumbing.Value);
        return saved.IsSuccess
            ? plumbing
            : Result.Failure<List<Plumbing>>(saved.Error);
    }
}