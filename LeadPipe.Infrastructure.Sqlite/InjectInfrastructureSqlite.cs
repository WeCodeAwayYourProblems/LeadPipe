using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Settings;
using LeadPipe.Infrastructure.Sqlite.Context;
using LeadPipe.Infrastructure.Sqlite.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LeadPipe.Infrastructure.Sqlite;

public static class InjectInfrastructureSqlite
{
    public static IServiceCollection AddInfrastructureSqlite(this IServiceCollection services, IDwhSettings settings)
    {
        // Add Databases
        services.AddDbContext<PlumbingContext>(options =>
        {
            string? connectionString = settings.PlumbingContext!;
            options.UseSqlite(connectionString);
        });

        // Add Repositories
        RegisterRepositories(services);

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
