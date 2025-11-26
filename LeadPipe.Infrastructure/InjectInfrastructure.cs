using LeadPipe.Application.Service;
using LeadPipe.Infrastructure.Database;
using LeadPipe.Infrastructure.Repository;
using LeadPipe.Infrastructure.Service;
using LeadPipe.Infrastructure.Settings;
using LeadPipe.Infrastructure.Translate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

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
        services.AddScoped<IFileService, FileService>();

        // Repositories
        RegisterRepositories(services);

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
            c.DefaultRequestHeaders.Add("Authorization", settings.LeafTokenType!);
        });

        // Add Lab Client
        services.AddHttpClient(settings.LabName!, c =>
        {
            c.BaseAddress = new Uri(settings.LabUri!);
            c.DefaultRequestHeaders.Add("Accept", settings.LabAccept!);
            c.DefaultRequestHeaders.Add("Authorization", settings.LabToken!);
        });

        // Add Databases
        services.AddDbContext<PlumbingContext>(options =>
        {
            string? connectionString = configuration.GetConnectionString("PlumbingContext");
            options.UseSqlite(connectionString);
        });

        return services;
    }
    private static void RegisterRepositories(IServiceCollection services)
    {
        Assembly assembly = typeof(PlumbingRepository).Assembly;

        // Find all non-abstract classes that inherit from PlumbingContextRepository<T>
        IEnumerable<Type> implementations = assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
                        t.BaseType != null &&
                        t.BaseType.IsGenericType &&
                        t.BaseType.GetGenericTypeDefinition() == typeof(PlumbingContextRepository<>));

        foreach (Type implementation in implementations)
        {
            // Find the first interface implemented by this class that inherits IRepository<T>
            Type? iface = implementation
                .GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IRepository<>)
                    ||
                    i.GetInterfaces().Any(ii =>
                        ii.IsGenericType &&
                        ii.GetGenericTypeDefinition() == typeof(IRepository<>))
                );

            if (iface is not null)
            {
                services.AddScoped(iface, implementation);
            }
        }
    }
}
