using LeadPipe.Application.Service;
using LeadPipe.Infrastructure.Database;
using LeadPipe.Infrastructure.Service;
using LeadPipe.Infrastructure.Settings;
using LeadPipe.Infrastructure.Translate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Infrastructure;

public static class InjectInfrastructure
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IInfrastructureSettings settings, IConfiguration configuration)
    {
        // Format: services.AddScoped<Interface, Class>();

        // Services
        services.AddScoped<ICsvRwService, CsvRwService>();
        services.AddScoped<IFileConversionService, FileConversionService>();
        services.AddScoped<IJsonRwService, JsonRwService>();
        services.AddScoped<ILeafClientService, LeafClientService>();
        services.AddScoped<IYellerClientService, YellerClientService>();
        services.AddScoped<ICalliUpdateService, CalliUpdateFromFileService>();

        // Translations 
        services.AddScoped<IDtoToEntity, DtoToEntity>();
        services.AddScoped<IDtoToVo, DtoToVo>();
        services.AddScoped<IEntityToDto, EntityToDto>();
        services.AddScoped<IEntityToVo, EntityToVo>();
        services.AddScoped<IVoToDto, VoToDto>();
        services.AddScoped<IVoToEntity, VoToEntity>();

        // Logging
        services.AddTransient<LeafClientService>();

        // Add Leaf Client
        services.AddHttpClient();
        services.AddHttpClient(settings.LeafName!, c =>
        {
            c.BaseAddress = new Uri(settings.LeafBase!);
            c.DefaultRequestHeaders.Add("Accept", "application/json");
            c.DefaultRequestHeaders.Add("Authorization", settings.LeafTokenType);
        });

        // Add Databases
        services.AddDbContext<PlumbingContext>(options =>
        {
            string? connectionString = configuration.GetConnectionString("PlumbingContext");
            options.UseSqlite(connectionString);
        });

        return services;
    }
}
