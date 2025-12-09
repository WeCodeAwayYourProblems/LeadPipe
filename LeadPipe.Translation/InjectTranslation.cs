using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;
using LeadPipe.Translation.Primitives;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LeadPipe.Translation;

public static class InjectTranslation
{
    public static IServiceCollection AddTranslation(this IServiceCollection services, IInfrastructureSettings settings)
    {
        // Format: services.AddScoped<Interface, Class>();

        // Primitives
        services.AddScoped<IDateTimeTranslate, DateTimeTranslate>();

        // Translations 
        RegisterServices(services, typeof(IDtoToVo<,>));
        RegisterServices(services, typeof(IEntityToVo<,>));
        RegisterServices(services, typeof(IVoToDto<,>));
        RegisterServices(services, typeof(IVoToEntity<,>));

        return services;
    }
    private static void RegisterServices(IServiceCollection services, Type iface)
    {
        var assembly = Assembly.GetAssembly(typeof(InjectTranslation));
        if (assembly is null)
            return;

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface)
                continue;

            // Look for any implemented interface matching the open generic definition
            var targetInterface = type
                .GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == iface);

            if (targetInterface is null)
                continue;

            // Avoid duplicate registrations
            if (services.Any(sd => sd.ServiceType == targetInterface))
                continue;

            services.AddScoped(targetInterface, type);
        }
    }
}
