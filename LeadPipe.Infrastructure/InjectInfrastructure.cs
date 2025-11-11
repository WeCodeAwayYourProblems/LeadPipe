using Microsoft.Extensions.DependencyInjection;
using LeadPipe.Infrastructure.SettingsInterfaces;

namespace LeadPipe.Infrastructure;

public static class InjectInfrastructure
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IInfrastructureSettings settings)
    {
        // Format: services.AddScoped<Interface, Class>();
        //services.AddHttpClient(settings.name)
        services.AddHttpClient();
        return services;
    }
}
