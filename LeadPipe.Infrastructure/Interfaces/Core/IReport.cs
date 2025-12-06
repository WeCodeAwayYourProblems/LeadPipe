using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Interfaces.Core;

internal interface IReport<T>
{
    Task<Result> ReportData(List<T> d);
}
