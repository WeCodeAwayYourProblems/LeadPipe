using CSharpFunctionalExtensions;

namespace LeadPipe.Application.Service;

public interface IReportService<TVo> : IGetData<TVo>
{
    Task<Result> ReportAsync(List<TVo> data);
}
