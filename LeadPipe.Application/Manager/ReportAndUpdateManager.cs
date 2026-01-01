using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Manager;


public interface IReportAndUpdateManager
{
    Task<Result> Manage(Source source, bool refresh);
    Task<Result> Manage(bool refresh);
}
internal class ReportAndUpdateManager(
    IUpdateService<Call> updateCall,
    IReportService<Call> reportCall,
    IUpdateService<Sandwich> updateSandwich,
    IReportService<Sandwich> reportSandwich,
    IUpdateSourceFactory update,
    IReportSourceFactory report,
    IPlumbingAssociationService plumb
    ) : IReportAndUpdateManager
{
    private readonly IUpdateService<Call> _updateCall = updateCall;
    private readonly IReportService<Call> _reportCall = reportCall;
    private readonly IUpdateService<Sandwich> _updateSandwich = updateSandwich;
    private readonly IReportService<Sandwich> _reportSandwich = reportSandwich;
    private readonly IUpdateSourceFactory _update = update;
    private readonly IReportSourceFactory _report = report;
    private readonly IPlumbingAssociationService _plumb = plumb;

    public async Task<Result> Manage(Source source, bool refresh)
    {
        // Update
        IUpdateService<Plumbing> updateService = _update.GetService(source);
        Result<List<Plumbing>> updateData = refresh
            ? await updateService.UpdateDataAsync()
            : await updateService.GetDataAsync();
        Result savedData = updateData.IsSuccess
            ? await updateService.SaveDataAsync(updateData.Value)
            : updateData;

        // Call data
        Result<List<Call>> callData = refresh
            ? await _updateCall.UpdateDataAsync()
            : await _updateCall.GetDataAsync();
        Result savedCall = callData.IsSuccess
            ? await _updateCall.SaveDataAsync(callData.Value)
            : callData;

        // Sandwich data
        Result<List<Sandwich>> sandwichData = refresh
            ? await _updateSandwich.UpdateDataAsync()
            : await _updateSandwich.GetDataAsync();
        Result savedSandwich = sandwichData.IsSuccess
            ? await _updateSandwich.SaveDataAsync(sandwichData.Value)
            : sandwichData;

        // Associate
        Result associated = await _plumb.SaveLinksAsync();

        // Combine
        Result updateResult = Result.Combine(" | ", savedData, savedCall, savedSandwich, associated);
        if (updateResult.IsFailure)
            return updateResult;

        // Report
        IReportService<Plumbing> reportService = _report.GetService(source);
        Result reported = await reportService.SendReportAsync(updateData.Value);

        return reported;
    }
    public async Task<Result> Manage(bool refresh)
    {
        // Call data
        Result<List<Call>> callData = refresh
            ? await _updateCall.UpdateDataAsync()
            : await _updateCall.GetDataAsync();
        Result savedCall = callData.IsSuccess
            ? await _updateCall.SaveDataAsync(callData.Value)
            : callData;

        // Sandwich data
        Result<List<Sandwich>> sandwichData = refresh
            ? await _updateSandwich.UpdateDataAsync()
            : await _updateSandwich.GetDataAsync();
        Result savedSandwich = sandwichData.IsSuccess
            ? await _updateSandwich.SaveDataAsync(sandwichData.Value)
            : sandwichData;

        // Associate
        Result associated = await _plumb.SaveLinksAsync();

        Result updateResult = Result.Combine(" | ", savedCall, savedSandwich, associated);
        if (updateResult.IsFailure)
            return updateResult;

        // Update and Report
        List<Result> result = [];
        Source[] values = Enum.GetValues<Source>();
        foreach (Source source in values)
        {
            IUpdateService<Plumbing> update = _update.GetService(source);

            // Get data 
            Result<List<Plumbing>> data = refresh
                ? await update.UpdateDataAsync()
                : await update.GetDataAsync();

            // Save Data
            Result saved = data.IsSuccess
                ? await update.SaveDataAsync(data.Value)
                : data;

            if (data.IsFailure || saved.IsFailure)
            {
                result.Add(Result.Combine(" | ", data, saved);
                continue;
            }

            // Report Data
            IReportService<Plumbing> report = _report.GetService(source);
            Result reported = await report.SendReportAsync(data.Value);
            result.Add(reported);
        }

        // Return result
        return Result.Combine(" | ", [.. result]);
    }
}
