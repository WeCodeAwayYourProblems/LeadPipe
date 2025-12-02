using LeadPipe.Application.Manager;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace LeadPipe.Application;

public static class InjectApplication
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Format: services.AddScoped<Interface, Class>();  
        services.AddScoped<ILabManager, LabManager>();
        
        return services;
    }
}
