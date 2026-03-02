using LeadPipe.Infrastructure.Dto;

namespace LeadPipe.Infrastructure.Interfaces.Translate;

public interface IEntityToYellerReportFactory
{
    IEntityToReport<TEntity, ReportYeller> GetService<TEntity>();
}