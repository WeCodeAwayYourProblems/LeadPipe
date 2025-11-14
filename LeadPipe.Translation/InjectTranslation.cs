using LeadPipe.Translation.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Translation;

public static class InjectTranslation
{
    public static IServiceCollection AddTranslation(this IServiceCollection services)
    {
        // Format: services.AddScoped<Interface, Class>();
        services.AddScoped<IDateTimeTranslate, DateTimeTranslate>();
        return services;
    }
}
