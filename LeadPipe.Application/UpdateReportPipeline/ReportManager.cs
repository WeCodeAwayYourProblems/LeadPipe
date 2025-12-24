using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;

namespace LeadPipe.Application.UpdateReportPipeline;

internal interface IReportManager<TVo>
{
    Task<Result<List<TVo>>> ManageAsync();
}
internal abstract class ReportManager<TVo>(IReportService<TVo> report) : IReportManager<TVo>
{
    private readonly IReportService<TVo> _report = report;

    public async Task<Result<List<TVo>>> ManageAsync()
    {
        Result<List<TVo>> data = await _report.GetDataAsync();
        if (data.IsFailure) return data;

        var sent = await _report.SendReportAsync(data.Value);
        return sent.IsSuccess
            ? data
            : Result.Failure<List<TVo>>(sent.Error);
    }
}
