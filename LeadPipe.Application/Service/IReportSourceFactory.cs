using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Service;

public interface IReportSourceFactory
{
    public IReportService<Plumbing> GetService(Source source);
}