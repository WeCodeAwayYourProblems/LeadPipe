using Microsoft.Extensions.DependencyInjection;

namespace Template.Infrastructure;

public static class InjectInfrastructure
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Format: services.AddScoped<Interface, Class>();  
        return services;
    }
}
