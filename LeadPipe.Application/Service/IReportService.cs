using CSharpFunctionalExtensions;

namespace LeadPipe.Application.Service;

public interface IReportService<TVo> : IGetData<TVo>
{
    Task<Result> SendReportAsync(List<TVo> data);
}
