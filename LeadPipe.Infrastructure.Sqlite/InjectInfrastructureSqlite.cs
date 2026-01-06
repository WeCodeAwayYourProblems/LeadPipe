using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Settings;
using LeadPipe.Infrastructure.Sqlite.Context;
using LeadPipe.Infrastructure.Sqlite.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite;

public static class InjectInfrastructureSqlite
{
    public static IServiceCollection AddInfrastructureSqlite(this IServiceCollection services, IDwhSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.PlumbingConnectionString))
            throw new InvalidOperationException(
                "PlumbingConnectionString is not configured.");

        services.AddDbContext<PlumbingContext>((sp, options) =>
        {
            var cs = settings.PlumbingConnectionString;

            var dataSource = new SqliteConnectionStringBuilder(cs).DataSource;

            Directory.CreateDirectory(Path.GetDirectoryName(dataSource)!);

            options.UseSqlite(cs)
                .AddInterceptors(new SqlitePragmaInterceptor())
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging();
        });

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
