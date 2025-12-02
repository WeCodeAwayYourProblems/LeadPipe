using LeadPipe.Application.Service;
using LeadPipe.Infrastructure.Service;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Infrastructure;

public static class InjectInfrastructure
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IInfrastructureSettings settings)
    {
        // Format: services.AddScoped<Interface, Class>();

        // Services
        services.AddScoped<ICsvRwService, CsvRwService>();
        services.AddScoped<IFileConversionService, FileConversionService>();
        services.AddScoped<IJsonRwService, JsonRwService>();
        services.AddScoped<ILeafClientService, LeafClientService>();
        services.AddScoped<IYellerClientService, YellerClientService>();
        services.AddScoped<IPlumbingUpdateService, CalliUpdateFromFileService>();
        services.AddScoped<IFileService, FileService>();

        // Logging
        services.AddTransient<LeafClientService>();

        // Add Leaf Client
        services.AddHttpClient();
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

        return services;
    }
    
}
