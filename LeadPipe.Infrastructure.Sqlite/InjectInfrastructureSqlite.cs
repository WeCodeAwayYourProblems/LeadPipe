using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Settings;
using LeadPipe.Infrastructure.Sqlite.Context;
using LeadPipe.Infrastructure.Sqlite.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite;

public static class InjectInfrastructureSqlite
{
    public static IServiceCollection AddInfrastructureSqlite(this IServiceCollection services, IDwhSettings settings, IConfiguration config)
    {
        if (string.IsNullOrWhiteSpace(settings.PlumbingConnectionString))
            throw new InvalidOperationException(
                $"{nameof(settings.PlumbingConnectionString)} is not configured.");

        bool globalUseInMemory = config.GetValue<bool>("Ef:UseInMemoryDatabase");
        bool globalSensitiveLogging = config.GetValue<bool>("Ef:SensitiveLogging");
        LogLevel globalLogLevel = config.GetValue("Ef:LogLevel", LogLevel.Information);

        bool useInMemory = config.GetValue(
        "Ef:Sqlite:UseInMemoryConnection",
            globalUseInMemory);

        bool sensitiveLogging = config.GetValue(
            "Ef:Sqlite:SensitiveLogging",
            globalSensitiveLogging);

        LogLevel efLogLevel = config.GetValue(
            "Ef:Sqlite:LogLevel",
            globalLogLevel);

        services.AddSingleton<SqlitePragmaInterceptor>();
        if (useInMemory)
        {
            services.AddSingleton(_ =>
            {
                SqliteConnection conn = new("DataSource=:memory:");
                conn.Open();
                return conn;
            });
        }

        services.AddDbContext<PlumbingContext>((provider, options) =>
        {
            if (sensitiveLogging)
                options.EnableSensitiveDataLogging();

            options
                .AddInterceptors(provider.GetRequiredService<SqlitePragmaInterceptor>())
                .LogTo(Console.WriteLine, efLogLevel);

            if (useInMemory)
            {
                SqliteConnection conn = provider.GetRequiredService<SqliteConnection>();
                options.UseSqlite(conn);
            }
            else
            {
                string cs = settings.PlumbingConnectionString;
                string dataSource = new SqliteConnectionStringBuilder(cs).DataSource;

                Directory.CreateDirectory(Path.GetDirectoryName(dataSource)!);
                options.UseSqlite(cs);
            }
        });

        services.AddScoped<IRepository<CaliperEntity>, CaliperRepository>();
        services.AddScoped<IRepository<CornEntity>, CornRepository>();
        services.AddScoped<IRepository<PlumbingCaliperLink>, PlumbingCaliperLinkRepository>();
        services.AddScoped<IRepository<PlumbingEntity>, PlumbingRepository>();
        services.AddScoped<IRepository<SandCaliperLink>, SubsCaliperLinkRepository>();
        services.AddScoped<IRepository<SandCornLink>, SandCornLinkRepository>();
        services.AddScoped<IRepository<SandPlumbingLink>, SandPlumbingLinkRepository>();
        services.AddScoped<IRepository<SandEntity>, SandRepository>();
        services.AddScoped<ISyncStateRepository, SyncStateRepository>();
        services.AddScoped<IRepositoryFactory, RepositoryFactory>();

        return services;
    }

}
