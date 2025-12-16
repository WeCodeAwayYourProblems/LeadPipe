using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Infrastructure.Service.Report;


[SourceKey(Source.Calli)]
internal sealed class CalliReportService(
    [FromKeyedServices(Source.Calli)] ILoadData<Plumbing> load,
    ITransform<Plumbing, ReportFilePlumbing> transform,
    [FromKeyedServices(Source.Calli)] IReport<ReportFilePlumbing> report
    ) : ReportService<Plumbing, ReportFilePlumbing>(load, transform, report)
{ }

[SourceKey(Source.Lab)]
internal sealed class LabReportService(
    [FromKeyedServices(Source.Lab)] ILoadData<Plumbing> load,
    ITransform<Plumbing, ReportFilePlumbing> transform,
    [FromKeyedServices(Source.Lab)] IReport<ReportFilePlumbing> report
    ) : ReportService<Plumbing, ReportFilePlumbing>(load, transform, report)
{ }

[SourceKey(Source.Leaf)]
internal sealed class LeafReportService(
    [FromKeyedServices(Source.Leaf)] ILoadData<Plumbing> load,
    ITransform<Plumbing, ReportFilePlumbing> transform,
    [FromKeyedServices(Source.Leaf)] IReport<ReportFilePlumbing> report
    ) : ReportService<Plumbing, ReportFilePlumbing>(load, transform, report)
{ }

[SourceKey(Source.Leased)]
internal sealed class LeasedReportService(
    [FromKeyedServices(Source.Leased)] ILoadData<Plumbing> load,
    ITransform<Plumbing, ReportFilePlumbing> transform,
    [FromKeyedServices(Source.Leased)] IReport<ReportFilePlumbing> report
    ) : ReportService<Plumbing, ReportFilePlumbing>(load, transform, report)
{ }

[SourceKey(Source.Libacion)]
internal sealed class LibacionReportService(
    [FromKeyedServices(Source.Libacion)] ILoadData<Plumbing> load,
    ITransform<Plumbing, ReportFilePlumbing> transform,
    [FromKeyedServices(Source.Libacion)] IReport<ReportFilePlumbing> report
    ) : ReportService<Plumbing, ReportFilePlumbing>(load, transform, report)
{ }

[SourceKey(Source.Pan)]
internal sealed class PanReportService(
    [FromKeyedServices(Source.Pan)] ILoadData<Plumbing> load,
    ITransform<Plumbing, ReportFilePlumbing> transform,
    [FromKeyedServices(Source.Pan)] IReport<ReportFilePlumbing> report
    ) : ReportService<Plumbing, ReportFilePlumbing>(load, transform, report)
{ }

[SourceKey(Source.Yeller)]
internal sealed class YellerReportService(
    [FromKeyedServices(Source.Yeller)] ILoadData<Plumbing> load,
    ITransform<Plumbing, ReportYeller> transform,
    [FromKeyedServices(Source.Yeller)] IReport<ReportYeller> report
    ) : ReportService<Plumbing, ReportYeller>(load, transform, report)
{ }