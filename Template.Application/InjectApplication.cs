using Microsoft.Extensions.DependencyInjection;

namespace Template.Application;

public static class InjectApplication
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Format: services.AddScoped<Interface, Class>();  
        return services;
    }
}
