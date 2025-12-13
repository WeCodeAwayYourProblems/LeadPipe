using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Data.Persistence;
using LeadPipe.Infrastructure.Data.Source;
using LeadPipe.Infrastructure.Data.Transform;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Data;

internal static class ZInjectData
{
    public static IServiceCollection InjectData(this IServiceCollection services, IInfrastructureSettings settings)
    {
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
        services.AddKeyedScoped<ILoadData<Plumbing>, YellerLoadData>(Domain.ValueObjects.Source.Yeller);

        // Transformers
        services.AddScoped<ITransform<Plumbing, YellerReport>, YellerTransform>();

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
        return services;
    }
}
