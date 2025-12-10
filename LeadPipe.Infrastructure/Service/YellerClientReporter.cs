using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;

namespace LeadPipe.Infrastructure.Service;

[SourceKey(Source.Yeller)]
public class YellerClientReporter : IReport<YellerReport>
{
    public Task<Result> ReportData(List<YellerReport> d)
    {
        throw new NotImplementedException();
    }
}
