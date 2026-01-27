using LeadPipe.Infrastructure.Interfaces.Repository.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using LeadPipe.Infrastructure.MySql.Repository;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        bool globalUseInMemory = config.GetValue<bool>("Ef:UseInMemoryDatabase", false);
        bool globalSensitiveLogging = config.GetValue<bool>("Ef:SensitiveLogging", false);
        LogLevel globalLogLevel = config.GetValue("Ef:LogLevel", LogLevel.Information);

        bool useInMemory = config.GetValue(
        "Ef:MySql:UseInMemoryDatabase",
            globalUseInMemory);

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

        options.LogTo(Console.WriteLine, efLogLevel);

        // NOTE: InMemory DB is shared per service provider by name.
        // This is intentional for read-only semantics.
        if (useInMemory)
            options.UseInMemoryDatabase(inMemoryDbName);
        else
            options.UseMySql(
                connectionString,
                GetServerVersion(connectionString),
                mySqlOptions =>
                {
                    mySqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });

    }

    private static readonly ConcurrentDictionary<string, ServerVersion> _serverVersions = new();

    private static ServerVersion GetServerVersion(string connectionString)
    {
        return _serverVersions.GetOrAdd(
            connectionString,
            ServerVersion.AutoDetect);
    }

}
