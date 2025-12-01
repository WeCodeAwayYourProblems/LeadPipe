using LeadPipe.Infrastructure;
using LeadPipe.Infrastructure.MySql;
using LeadPipe.Infrastructure.Settings;
using LeadPipe.Infrastructure.Sqlite;
using LeadPipe.Infrastructure.Translate;
using LeadPipe.Translation.Primitives;
using LeadPipe.Translation.Translate;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Translation;

public static class InjectTranslation
{
    public static IServiceCollection AddTranslation(this IServiceCollection services, IInfrastructureSettings settings)
    {
        // Format: services.AddScoped<Interface, Class>();
        services.AddScoped<IDateTimeTranslate, DateTimeTranslate>();

        // Add translations
        // Translations 
        services.AddScoped<IDtoToVo, DtoToVo>();
        services.AddScoped<IEntityToVo, EntityToVo>();
        services.AddScoped<IVoToDto, VoToDto>();
        services.AddScoped<IVoToEntity, VoToEntity>();

        return services;
    }
}
