using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Translation;

public static class InjectTranslation
{
    public static IServiceCollection AddTranslation(this IServiceCollection services)
    {
        // Format: services.AddScoped<Interface, Class>();  
        return services;
    }
}
