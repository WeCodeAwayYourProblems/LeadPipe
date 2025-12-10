using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Service;

internal static class ZInjectServices
{
    public static IServiceCollection InjectServices(this IServiceCollection services, IInfrastructureSettings settings)
    {
        // Loggers 
        services.AddTransient<LabService>();
        services.AddTransient<LeafClientService>();
        services.AddTransient<YellerClientService>();

        // Keyed Services
        // Keyed update services
        services.AddKeyedScoped<IUpdateService<Plumbing>, CalliUpdateFromFileService>(Source.Calli);
        services.AddKeyedScoped<IUpdateService<Plumbing>, LabUpdateService>(Source.Lab);
        services.AddKeyedScoped<IUpdateService<Plumbing>, LeafUpdateService>(Source.Leaf);
        services.AddKeyedScoped<IUpdateService<Plumbing>, LeasedUpdateFromFileService>(Source.Leased);
        services.AddKeyedScoped<IUpdateService<Plumbing>, PanUpdateFromFileService>(Source.Pan);
        services.AddKeyedScoped<IUpdateService<Plumbing>, YellerUpdateService>(Source.Yeller);

        // Keyed Report services
        services.AddKeyedScoped<IReport<YellerReport>, YellerClientReporter>(Source.Yeller);

        // Scoped services
        services.AddScoped<ICsvRwService, CsvRwService>();
        services.AddScoped<IFileRWService, FileConversionService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IJsonRwService, JsonRwService>();
        services.AddScoped<ILabService, LabService>();
        services.AddScoped<ILeafService, LeafClientService>();
        services.AddScoped<IPlumbingAssociationService, PlumbingAssociationService>();
        services.AddScoped<IYellerService, YellerClientService>();
        services.AddScoped<IYellerReportService, YellerReportService>();

        return services;
    }
}
