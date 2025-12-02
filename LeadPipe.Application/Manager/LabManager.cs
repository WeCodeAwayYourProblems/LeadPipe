using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Manager;

public class LabManager(ILabService lab, IPlumbingUpdateService plumbing) : ILabManager
{
    private readonly ILabService _lab = lab;
    private readonly IPlumbingUpdateService _plumbing = plumbing;
    public async Task<Result<List<Plumbing>>> Manage(int errorLimit = 5, bool update = true)
    {
        Result<List<Plumbing>> labresult = update
            ? await _lab.UpdateDataAsync(errorLimit: errorLimit)
            : await _lab.GetLabsAsync(errorLimit: errorLimit);

        if (labresult.IsFailure)
            return labresult;

        Result saved = await _plumbing.SaveDataAsync(labresult.Value);
        Result<List<Plumbing>> result = saved.IsSuccess
            ? labresult.Value
            : Result.Failure<List<Plumbing>>(saved.Error);

        return result;
    }
}
