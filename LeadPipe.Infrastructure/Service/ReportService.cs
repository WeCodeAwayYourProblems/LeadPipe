using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Infrastructure.Interfaces.Core;

namespace LeadPipe.Infrastructure.Service;

internal abstract class ReportService<TVo, TReport>(
    ITransform<TVo, TReport> transform,
    IReport<TReport> report
    ) : IReportService<TVo>
{
    private readonly ITransform<TVo, TReport> _transform = transform;
    private readonly IReport<TReport> _report = report;
    public async Task<Result<List<TVo>>> GetDataAsync()
    {
        return await _transform.LoadAsync();
    }

    public async Task<Result> SendReportAsync(List<TVo> data)
    {
        Result<List<TReport>> d = await _transform.TransformAsync(data);
        if (d.IsFailure)
            return d;
        return await _report.ReportData(d.Value);
    }
}