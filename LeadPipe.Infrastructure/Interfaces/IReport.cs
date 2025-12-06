using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Interfaces;

internal interface IReport<T>
{
    Task<Result> ReportData(List<T> d);
}
