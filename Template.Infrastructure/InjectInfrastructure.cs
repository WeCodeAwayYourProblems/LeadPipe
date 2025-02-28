using Microsoft.Extensions.DependencyInjection;
using Template.Infrastructure.SettingsInterfaces;

namespace Template.Infrastructure;

public static class InjectInfrastructure
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IInfrastructureSettings settings)
    {
        // Format: services.AddScoped<Interface, Class>();
        services.AddHttpClient();
        services.AddHttpClient(settings.name)
        return services;
    }
}
