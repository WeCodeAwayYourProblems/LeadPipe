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

        services.AddScoped<ICaliperRepository, CaliperRepository>();
        services.AddScoped<ICornRepository, CornRepository>();
        services.AddScoped<IPlumbingCaliperLinkRepository, PlumbingCaliperLinkRepository>();
        services.AddScoped<IPlumbingRepository, PlumbingRepository>();
        services.AddScoped<ISandCaliperLinkRepository, SubsCaliperLinkRepository>();
        services.AddScoped<ISandCornLinkRepository, SandCornLinkRepository>();
        services.AddScoped<ISandPlumbingLinkRepository, SandPlumbingLinkRepository>();
        services.AddScoped<ISandRepository, SandRepository>();
        services.AddScoped<ISyncStateRepository, SyncStateRepository>();

        return services;
    }

}
