using LeadPipe.Infrastructure.Interfaces.Repository;
using LeadPipe.Infrastructure.MySql.Context;
using LeadPipe.Infrastructure.MySql.Repository;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System.Collections.Concurrent;

namespace LeadPipe.Infrastructure.MySql;

public static class InjectInfrastructureMySql
{
    public static IServiceCollection AddInfrastructureMySql(this IServiceCollection services, IMySqlSettings settings, IConfiguration config)
    {
        #region Register Contexts

        // Register MySqlContext for Schema1
        if (string.IsNullOrWhiteSpace(settings.Schema1ConnectionString))
            throw new InvalidOperationException("MySqlConnectionString for Schema1 is missing.");

        bool globalUseInMemory = config.GetValue("Ef:UseInMemoryDatabase", false);
        bool globalSensitiveLogging = config.GetValue("Ef:SensitiveLogging", false);
        LogLevel globalLogLevel = config.GetValue("Ef:LogLevel", LogLevel.Information);

        bool requestedInMemory = config.GetValue(
            "Ef:MySql:UseInMemoryDatabase",
            globalUseInMemory);
        bool useInMemory = requestedInMemory;
        if (!useInMemory)
        {
            bool isDevelopment = config.GetValue("DOTNET_ENVIRONMENT", "Production") == "Development";
            bool canConnect = CanConnect(settings.Schema1ConnectionString);

            if (!canConnect && !isDevelopment)
                throw new InvalidOperationException("Database unavailable.");

            useInMemory = !canConnect;
        }

        // Log decision about using in memory DB
        string whyNotAvailable = requestedInMemory ? "explicitly configured" : "unavailable";
        Console.WriteLine(useInMemory
            ? $"Using InMemoryDatabase (MySql {whyNotAvailable})"
            : "Using MySql Database");

        bool sensitiveLogging = config.GetValue(
            "Ef:MySql:SensitiveLogging",
            globalSensitiveLogging);

        LogLevel efLogLevel = config.GetValue(
            "Ef:MySql:LogLevel",
            globalLogLevel);

        services.AddDbContext<MySqlSchema1Context>((sp, options) =>
        {
            ConfigureMySqlOrInMemory<MySqlSchema1Context>(
                options,
                settings.Schema1ConnectionString,
                inMemoryDbName: "Schema1",
                useInMemory,
                sensitiveLogging,
                efLogLevel);
        });

        // Register MySqlContext for Schema2 
        if (string.IsNullOrWhiteSpace(settings.Schema2ConnectionString))
            throw new InvalidOperationException("MySqlConnectionString for Schema2 is missing.");

        services.AddDbContext<MySqlSchema2Context>((sp, options) =>
        {
            ConfigureMySqlOrInMemory<MySqlSchema2Context>(
                options,
                settings.Schema2ConnectionString,
                inMemoryDbName: "Schema2",
                useInMemory,
                sensitiveLogging,
                efLogLevel);
        });

        // Register MySqlContext for Schema3
        if (string.IsNullOrWhiteSpace(settings.Schema3ConnectionString))
            throw new InvalidOperationException("MySqlConnectionString for Schema3 is missing.");

        services.AddDbContext<MySqlSchema3Context>((sp, options) =>
        {
            ConfigureMySqlOrInMemory<MySqlSchema3Context>(
                options,
                settings.Schema3ConnectionString,
                inMemoryDbName: "Schema3",
                useInMemory,
                sensitiveLogging,
                efLogLevel);
        });

        #endregion

        #region Register Repositories
        services.AddScoped<ICaliperMySqlRepository, CaliperMySqlRepository>();
        services.AddScoped<ICornMySqlRepository, CornMySqlRepository>();
        services.AddScoped<ICustardMySqlRepository, CustardMySqlRepository>();
        services.AddScoped<ISandMySqlRepository, SandMySqlRepository>();
        services.AddScoped<ISummaryMySqlRepository, SummaryMySqlRepository>();
        services.AddScoped<ITranscriptionMySqlRepository, TranscriptionMySqlRepository>();

        #endregion

        return services;
    }

    private static void ConfigureMySqlOrInMemory<TContext>(
        DbContextOptionsBuilder options,
        string connectionString,
        string inMemoryDbName,
        bool useInMemory,
        bool sensitiveLogging,
        LogLevel efLogLevel
    ) where TContext : MySqlBaseContext
    {
        if (sensitiveLogging)
            options.EnableSensitiveDataLogging();

        options.LogTo(msg => System.Diagnostics.Debug.WriteLine(msg), efLogLevel);

        // NOTE: InMemory DB is shared per service provider by name.
        // This is intentional for read-only semantics.
        if (useInMemory)
        {
            options.UseInMemoryDatabase(inMemoryDbName);
        }
        else
        {
            options.UseMySql(
                connectionString,
                GetServerVersion(connectionString),
                mySqlOptions =>
                {
                    mySqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });
        }

    }

    private static readonly ConcurrentDictionary<string, ServerVersion> _serverVersions = new();

    private static ServerVersion GetServerVersion(string connectionString)
    {
        return _serverVersions.GetOrAdd(
            connectionString,
            ServerVersion.AutoDetect);
    }
    private static bool CanConnect(string connectionString)
    {
        try
        {
            using var connection = new MySqlConnection(connectionString + ";Connection Timeout=2;");
            connection.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
