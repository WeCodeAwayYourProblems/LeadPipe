using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Data.Source;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Service;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace LeadPipe.Infrastructure;

public static class InjectInfrastructure
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IInfrastructureSettings settings)
    {
        // Format: services.AddScoped<Interface, Class>();

        // Loggers
        
        // Services
        services.AddScoped<ICsvRwService, CsvRwService>();
        services.AddScoped<IFileRWService, FileConversionService>();
        services.AddScoped<IJsonRwService, JsonRwService>();
        services.AddScoped<ILeafService, LeafClientService>();
        services.AddScoped<IYellerService, YellerClientService>();
        services.AddScoped<IFileService, FileService>();

        // Data sources
        RegisterServices(services, typeof(IDataSourceAsync<>));

        services.AddScoped<IDataSourceAsync<CalliDto>>(sp =>
            new CalliFileDataSource(
                new FileInfo(settings.CalliLocation!),
                sp.GetRequiredService<ICsvRwService>(),
                sp.GetRequiredService<IJsonRwService>(),
                sp.GetRequiredService<ILogger<CalliFileDataSource>>()
            ));

        services.AddScoped<IDataSourceAsync<LeasedDto>>(sp =>
            new LeasedFileDataSource(
                new FileInfo(settings.LeasedLocation!),
                sp.GetRequiredService<ICsvRwService>(),
                sp.GetRequiredService<IJsonRwService>(),
                sp.GetRequiredService<ILogger<LeasedFileDataSource>>()
            ));

        services.AddScoped<IDataSourceAsync<LibacionDto>>(sp =>
            new LibacionFileDataSource(
                new FileInfo(settings.LibacionLocation!),
                sp.GetRequiredService<ICsvRwService>(),
                sp.GetRequiredService<IJsonRwService>(),
                sp.GetRequiredService<ILogger<LibacionFileDataSource>>()
            ));

        services.AddScoped<IDataSourceAsync<PanDto>>(sp =>
            new PanFileDataSource(
                new FileInfo(settings.PanLocation!),
                sp.GetRequiredService<ICsvRwService>(),
                sp.GetRequiredService<IJsonRwService>(),
                sp.GetRequiredService<ILogger<PanFileDataSource>>()
            ));

        // Persistence
        RegisterServices(services, typeof(IDataPersistence<>));

        // Keyed Services
        RegisterKeyedServices<SourceKeyAttribute>(services, typeof(IUpdateService<>));

        // Add Leaf Client
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

        // Add Yeller Client
        services.AddHttpClient(settings.YellerName!, c =>
        {
            c.BaseAddress = new Uri(settings.YellerBase!);
            c.DefaultRequestHeaders.Add("Accept", "application/json");
            c.DefaultRequestHeaders.Add("Authorization", settings.YellerToken!);
        });

        return services;
    }

    private static void RegisterServices(IServiceCollection services, Type iface)
    {
        // Get only the assembly that contains your infrastructure registrations
        var assembly = Assembly.GetAssembly(typeof(InjectInfrastructure));
        if (assembly is null)
            return;

        IEnumerable<Type> types = assembly
            .GetTypes()
            .Where(t => t is not null)
            .OfType<Type>();  // ensures non-nullable Type

        foreach (Type type in types)
        {
            if (type.IsAbstract || type.IsInterface)
                continue;

            // Skip data sources that require FileInfo (they are manually registered)
            if (type.GetConstructors().Any(c => c.GetParameters().Any(p => p.ParameterType == typeof(FileInfo))))
                continue;

            // Look for an interface matching iface (like IDataSourceAsync<> or IDataPersistence<>)
            Type? targetInterface = type
                .GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == iface
                );

            if (targetInterface is null)
                continue;

            // Avoid duplicate registrations
            if (services.Any(sd => sd.ServiceType == targetInterface))
                continue;

            services.AddScoped(targetInterface, type);
        }
    }
    private static void RegisterKeyedServices<TAttribute>(IServiceCollection services, Type iface) where TAttribute : Attribute, ISourceKeyAttribute
    {
        var assembly = Assembly.GetAssembly(typeof(InjectInfrastructure));
        if (assembly is null)
            return;
        foreach (Type? type in assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface))
        {
            var targetInterface = type.GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == iface &&
                    i.GenericTypeArguments.Length == 1 &&
                    i.GenericTypeArguments[0] == typeof(Plumbing)
                );
            if (targetInterface is null)
                continue;
            var keyAttr = type.GetCustomAttribute<TAttribute>();
            if (keyAttr is null)
                continue;
            services.AddKeyedScoped(typeof(IUpdateService<Plumbing>), keyAttr.Key, type);
        }
    }

}
