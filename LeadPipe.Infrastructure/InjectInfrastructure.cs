using Microsoft.Extensions.DependencyInjection;
using LeadPipe.Infrastructure.SettingsInterfaces;
using LeadPipe.Application.Services;

namespace LeadPipe.Infrastructure;

public static class InjectInfrastructure
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IInfrastructureSettings settings)
    {
        // Format: services.AddScoped<Interface, Class>();
        services.AddScoped<ILeafClientService, ILeafClientService>();

        // Add Leaf Client
        services.AddHttpClient(settings.LeafName!, c=>
        {
            c.BaseAddress = new Uri(settings.LeafBase!);
            c.DefaultRequestHeaders.Add("Accept", "application/json");
            c.DefaultRequestHeaders.Add("Authorization", settings.LeafTokenType);
        });

        services.AddHttpClient();
        return services;
    }
}
