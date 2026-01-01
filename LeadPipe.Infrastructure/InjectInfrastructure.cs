using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Data.Load;
using LeadPipe.Infrastructure.Data.Persistence;
using LeadPipe.Infrastructure.Data.Source;
using LeadPipe.Infrastructure.Data.Transform;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Factory;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Service;
using LeadPipe.Infrastructure.Service.Report;
using LeadPipe.Infrastructure.Service.Update;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Infrastructure;

public static class InjectInfrastructure
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IInfrastructureSettings settings)
    {
        // Format: services.AddScoped<Interface, Class>();

        // *****************************************
        #region ADD DATA

        // Data Persistence
        services.AddScoped<IDataPersistence<CallEntity>, CallEntityPersistence>();
        services.AddScoped<IDataPersistence<PlumbingCallLink>, PlumbingCallLinkPersistence>();
        services.AddScoped<IDataPersistence<PlumbingEntity>, PlumbingPersistence>();
        services.AddScoped<IDataPersistence<CallSubsLink>, SubsCallLinkPersistence>();
        services.AddScoped<IDataPersistence<SubsEntity>, SubsEntityPersistence>();
        services.AddScoped<IDataPersistence<SubsPlumbingLink>, SubsPlumbingLinkPersistence>();
        services.AddScoped<IDataPersistence<Call>, CallPersistence>();
        services.AddScoped<IDataPersistence<Sandwich>, SandwichPersistence>();

        // Keyed Sources
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadCalli>(Source.Calli);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadLab>(Source.Lab);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadLeaf>(Source.Leaf);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadLeased>(Source.Leased);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadLibacion>(Source.Libacion);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadPan>(Source.Pan);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadYeller>(Source.Yeller);

        // Transformers
        services.AddScoped<ITransform<Plumbing, ReportYeller>, TransformYellerReport>();
        services.AddScoped<ITransform<Plumbing, ReportPlumbing>, TransformPlumbingReport>();

        // Loaders
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadCalli>(Source.Calli);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadLab>(Source.Lab);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadLeaf>(Source.Leaf);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadLeased>(Source.Leased);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadLibacion>(Source.Libacion);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadPan>(Source.Pan);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadYeller>(Source.Yeller);

        // Data Sources
        services.AddScoped<IDataSourceAsync<LabDto>, LabDataSource>();
        services.AddScoped<IDataSourceAsync<LeafDto>, LeafDataSource>();
        services.AddScoped<IDataSourceAsync<YellerDto>, YellerDataSource>();
        services.AddScoped<IDataSourceAsync<CalliDto>, CalliFileDataSource>();
        services.AddScoped<IDataSourceAsync<LeasedDto>, LeasedFileDataSource>();
        services.AddScoped<IDataSourceAsync<LibacionDto>, LibacionFileDataSource>();
        services.AddScoped<IDataSourceAsync<PanDto>, PanFileDataSource>();
        services.AddScoped<IDataSourceAsync<CallMySqlEntity>, CallMySqlDataSource>();
        services.AddScoped<IDataSourceAsync<SubMySqlEntity>, SubMySqlDataSource>();
        #endregion

        // *****************************************
        #region ADD SERVICES

        // Loggers 
        services.AddTransient<LabService>();
        services.AddTransient<LeafClientService>();
        services.AddTransient<YellerClientService>();

        // Keyed update services
        services.AddKeyedScoped<IUpdateService<Plumbing>, CalliUpdateFromFileService>(Source.Calli);
        services.AddKeyedScoped<IUpdateService<Plumbing>, LabUpdateService>(Source.Lab);
        services.AddKeyedScoped<IUpdateService<Plumbing>, LibacionUpdateService>(Source.Libacion);
        services.AddKeyedScoped<IUpdateService<Plumbing>, LeafUpdateService>(Source.Leaf);
        services.AddKeyedScoped<IUpdateService<Plumbing>, LeasedUpdateFromFileService>(Source.Leased);
        services.AddKeyedScoped<IUpdateService<Plumbing>, PanUpdateFromFileService>(Source.Pan);
        services.AddKeyedScoped<IUpdateService<Plumbing>, YellerUpdateService>(Source.Yeller);

        // Nonkeyed update services
        services.AddScoped<IUpdateService<Call>, CallsUpdateService>();
        services.AddScoped<IUpdateService<Sandwich>, SandwichUpdateService>();

        // Keyed Report services
        services.AddKeyedScoped<IReportService<Plumbing>, CalliReportService>(Source.Calli);
        services.AddKeyedScoped<IReportService<Plumbing>, LabReportService>(Source.Lab);
        services.AddKeyedScoped<IReportService<Plumbing>, LeafReportService>(Source.Leaf);
        services.AddKeyedScoped<IReportService<Plumbing>, LeasedReportService>(Source.Leased);
        services.AddKeyedScoped<IReportService<Plumbing>, LibacionReportService>(Source.Libacion);
        services.AddKeyedScoped<IReportService<Plumbing>, PanReportService>(Source.Pan);
        services.AddKeyedScoped<IReportService<Plumbing>, YellerReportService>(Source.Yeller);

        // Factories for keyed services
        services.AddScoped<IReportSourceFactory, ReportSourceFactory>();
        services.AddScoped<IUpdateSourceFactory, UpdateSourceFactory>();

        // Report services
        services.AddKeyedScoped<IReport<ReportPlumbing>, CalliReporter>(Source.Calli);
        services.AddKeyedScoped<IReport<ReportPlumbing>, LabReporter>(Source.Lab);
        services.AddKeyedScoped<IReport<ReportPlumbing>, LeasedReporter>(Source.Leased);
        services.AddKeyedScoped<IReport<ReportPlumbing>, LeafReporter>(Source.Leaf);
        services.AddKeyedScoped<IReport<ReportPlumbing>, LibacionReporter>(Source.Libacion);
        services.AddKeyedScoped<IReport<ReportPlumbing>, PanReporter>(Source.Pan);
        services.AddKeyedScoped<IReport<ReportPlumbing>, YellerReporter>(Source.Yeller);

        services.AddScoped<IReport<ReportYeller>, YellerClientReporter>();

        // Scoped services
        services.AddScoped<ICsvRwService, CsvRwService>();
        services.AddScoped<IFileRWService, FileConversionService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IJsonRwService, JsonRwService>();
        services.AddScoped<ILabService, LabService>();
        services.AddScoped<ILeafService, LeafClientService>();
        services.AddScoped<IPlumbingAssociationService, PlumbingAssociationService>();
        services.AddScoped<IYellerService, YellerClientService>();

        #endregion

        // *****************************************
        #region ADD CLIENTS

        // Add Leaf Client
        if (string.IsNullOrWhiteSpace(settings.LeafName))
            throw new Exception($"{nameof(settings.LeafName)} cannot be null");
        if (string.IsNullOrWhiteSpace(settings.LeafBase))
            throw new Exception($"{nameof(settings.LeafBase)} cannot be null");
        if (string.IsNullOrWhiteSpace(settings.LeafTokenType))
            throw new Exception($"{nameof(settings.LeafTokenType)} cannot be null");
        services.AddHttpClient(settings.LeafName, c =>
        {
            c.BaseAddress = new Uri(settings.LeafBase);
            c.DefaultRequestHeaders.Add("Accept", "application/json");
            c.DefaultRequestHeaders.Add("Authorization", settings.LeafTokenType);
        });

        // Add Lab Client
        if (string.IsNullOrWhiteSpace(settings.LabName))
            throw new Exception($"{nameof(settings.LabName)} cannot be null");
        if (string.IsNullOrWhiteSpace(settings.LabUri))
            throw new Exception($"{nameof(settings.LabUri)} cannot be null");
        if (string.IsNullOrWhiteSpace(settings.LabAccept))
            throw new Exception($"{nameof(settings.LabAccept)} cannot be null");
        if (string.IsNullOrWhiteSpace(settings.LabToken))
            throw new Exception($"{nameof(settings.LabToken)} cannot be null");
        services.AddHttpClient(settings.LabName, c =>
        {
            c.BaseAddress = new Uri(settings.LabUri);
            c.DefaultRequestHeaders.Add("Accept", settings.LabAccept);
            c.DefaultRequestHeaders.Add("Authorization", settings.LabToken);
        });

        // Add Yeller Client
        if (string.IsNullOrWhiteSpace(settings.YellerName))
            throw new Exception($"{nameof(settings.YellerName)} cannot be null");
        if (string.IsNullOrWhiteSpace(settings.YellerBase))
            throw new Exception($"{nameof(settings.YellerBase)} cannot be null");
        if (string.IsNullOrWhiteSpace(settings.YellerToken))
            throw new Exception($"{nameof(settings.YellerToken)} cannot be null");
        services.AddHttpClient(settings.YellerName, c =>
        {
            c.BaseAddress = new Uri(settings.YellerBase);
            c.DefaultRequestHeaders.Add("Accept", "application/json");
            c.DefaultRequestHeaders.Add("Authorization", settings.YellerToken);
        });

        #endregion

        return services;
    }
}
