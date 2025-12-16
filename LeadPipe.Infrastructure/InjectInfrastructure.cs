using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Data.Load;
using LeadPipe.Infrastructure.Data.Persistence;
using LeadPipe.Infrastructure.Data.Source;
using LeadPipe.Infrastructure.Data.Transform;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Service;
using LeadPipe.Infrastructure.Service.Report;
using LeadPipe.Infrastructure.Service.Update;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure;

public static class InjectInfrastructure
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IInfrastructureSettings settings)
    {
        // Format: services.AddScoped<Interface, Class>();

        #region ADD DATA
        // *****************************************

        // Data Persistence
        services.AddScoped<IDataPersistence<CallEntity>, CallEntityPersistence>();
        services.AddScoped<IDataPersistence<CallMySqlEntity>, CallMySqlPersistence>();
        services.AddScoped<IDataPersistence<CustomerMySqlEntity>, CustomerMySqlEntityPersistence>();
        services.AddScoped<IDataPersistence<PlumbingCallLink>, PlumbingCallLinkPersistence>();
        services.AddScoped<IDataPersistence<PlumbingEntity>, PlumbingPersistence>();
        services.AddScoped<IDataPersistence<SubMySqlEntity>, SubMySqlEntityPersistence>();
        services.AddScoped<IDataPersistence<CallSubsLink>, SubsCallLinkPersistence>();
        services.AddScoped<IDataPersistence<SubsPlumbingLink>, SubsPlumbingLinkPersistence>();
        services.AddScoped<IDataPersistence<SummaryMySqlEntity>, SummaryMySqlEntityPersistence>();

        // Data Sources
        services.AddScoped<IDataSourceAsync<LabDto>, LabDataSource>();
        services.AddScoped<IDataSourceAsync<LeafDto>, LeafDataSource>();
        services.AddScoped<IDataSourceAsync<YellerDto>, YellerDataSource>();

        // Keyed Sources
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadCalli>(Source.Calli);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadLab>(Source.Lab);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadLeaf>(Source.Leaf);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadLeased>(Source.Leased);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadLibacion>(Source.Libacion);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadPan>(Source.Pan);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadYeller>(Source.Yeller);

        // Transformers
        services.AddScoped<ITransform<Plumbing, ReportYeller>, YellerTransform>();
        services.AddScoped<ITransform<Plumbing, ReportFilePlumbing>, PlumbingTransform>();

        // Loaders
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadCalli>(Source.Calli);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadLab>(Source.Lab);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadLeaf>(Source.Leaf);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadLeased>(Source.Leased);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadLibacion>(Source.Libacion);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadPan>(Source.Pan);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadYeller>(Source.Yeller);

        // Data sources with file locations
        services.AddScoped<IDataSourceAsync<CalliDto>>(sp =>
            new CalliFileDataSource(
                new FileInfo(settings.CalliLocation!),
                sp.GetRequiredService<ICsvRwService>(),
                sp.GetRequiredService<IJsonRwService>(),
                sp.GetRequiredService<ILogger<CalliFileDataSource>>()
            ));

        services.AddScoped<IDataSourceAsync<LeasedDto>>(sp =>
            new LeasedFileDataSource(
                new FileInfo(settings.LeasedLocation!),
                sp.GetRequiredService<ICsvRwService>(),
                sp.GetRequiredService<IJsonRwService>(),
                sp.GetRequiredService<ILogger<LeasedFileDataSource>>()
            ));

        services.AddScoped<IDataSourceAsync<LibacionDto>>(sp =>
            new LibacionFileDataSource(
                new FileInfo(settings.LibacionLocation!),
                sp.GetRequiredService<ICsvRwService>(),
                sp.GetRequiredService<IJsonRwService>(),
                sp.GetRequiredService<ILogger<LibacionFileDataSource>>()
            ));

        services.AddScoped<IDataSourceAsync<PanDto>>(sp =>
            new PanFileDataSource(
                new FileInfo(settings.PanLocation!),
                sp.GetRequiredService<ICsvRwService>(),
                sp.GetRequiredService<IJsonRwService>(),
                sp.GetRequiredService<ILogger<PanFileDataSource>>()
            ));
        #endregion

        #region ADD SERVICES
        // *****************************************

        // Loggers 
        services.AddTransient<LabService>();
        services.AddTransient<LeafClientService>();
        services.AddTransient<YellerClientService>();

        // Keyed Services
        // Keyed update services
        services.AddKeyedScoped<IUpdateService<Plumbing>, CalliUpdateFromFileService>(Source.Calli);
        services.AddKeyedScoped<IUpdateService<Plumbing>, LabUpdateService>(Source.Lab);
        services.AddKeyedScoped<IUpdateService<Plumbing>, LibacionUpdateService>(Source.Libacion);
        services.AddKeyedScoped<IUpdateService<Plumbing>, LeafUpdateService>(Source.Leaf);
        services.AddKeyedScoped<IUpdateService<Plumbing>, LeasedUpdateFromFileService>(Source.Leased);
        services.AddKeyedScoped<IUpdateService<Plumbing>, PanUpdateFromFileService>(Source.Pan);
        services.AddKeyedScoped<IUpdateService<Plumbing>, YellerUpdateService>(Source.Yeller);

        // Keyed Report services
        services.AddKeyedScoped<IReportService<Plumbing>, CalliReportService>(Source.Calli);
        services.AddKeyedScoped<IReportService<Plumbing>, YellerReportService>(Source.Yeller);

        // Non-keyed report services
        services.AddKeyedScoped<IReport<ReportYeller>, YellerClientReporter>(Source.Yeller);
        services.AddKeyedScoped<IReport<ReportFilePlumbing>, CalliReporter>(Source.Calli);

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

        #region ADD CLIENTS
        // *****************************************

        // Add Leaf Client
        services.AddHttpClient(settings.LeafName!, c =>
        {
            c.BaseAddress = new Uri(settings.LeafBase!);
            c.DefaultRequestHeaders.Add("Accept", "application/json");
            c.DefaultRequestHeaders.Add("Authorization", settings.LeafTokenType!);
        });

        // Add Lab Client
        services.AddHttpClient(settings.LabName!, c =>
        {
            c.BaseAddress = new Uri(settings.LabUri!);
            c.DefaultRequestHeaders.Add("Accept", settings.LabAccept!);
            c.DefaultRequestHeaders.Add("Authorization", settings.LabToken!);
        });

        // Add Yeller Client
        services.AddHttpClient(settings.YellerName!, c =>
        {
            c.BaseAddress = new Uri(settings.YellerBase!);
            c.DefaultRequestHeaders.Add("Accept", "application/json");
            c.DefaultRequestHeaders.Add("Authorization", settings.YellerToken!);
        });

        #endregion

        return services;
    }
}
