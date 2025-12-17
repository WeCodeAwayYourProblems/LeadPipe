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
        services.AddScoped<ICallRepository, CallRepository>();
        services.AddScoped<IPlumbingCallLinkRepository, PlumbingCallLinkRepository>();
        services.AddScoped<IPlumbingRepository, PlumbingRepository>();
        services.AddScoped<ISubsCallLinkRepository, SubsCallLinkRepository>();
        services.AddScoped<ISubsPlumbingLinkRepository, SubsPlumbingLinkRepository>();
        services.AddScoped<ISubsRepository, SubsRepository>();
        services.AddScoped<ISyncStateRepository, SyncStateRepository>();

        return services;
    }
}
