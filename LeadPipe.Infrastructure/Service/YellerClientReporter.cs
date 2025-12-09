using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;

namespace LeadPipe.Infrastructure.Service;

public class YellerClientReporter : IReport<YellerReport>
{
    public Task<Result> ReportData(List<YellerReport> d)
    {
        throw new NotImplementedException();
    }
}
