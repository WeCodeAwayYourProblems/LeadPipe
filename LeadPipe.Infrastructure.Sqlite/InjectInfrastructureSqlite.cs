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
    public static IServiceCollection AddInfrastructureSqlite(this IServiceCollection services, IInfrastructureSettings settings, IConfiguration config)
    {
        #region Register Db Context
        if (string.IsNullOrWhiteSpace(settings.PlumbingConnectionString))
            throw new InvalidOperationException(
                $"{nameof(settings.PlumbingConnectionString)} is not configured.");

        bool globalUseInMemory = settings.Ef is not null && settings.Ef.UseInMemoryDatabase;
        bool globalSensitiveLogging = settings.Ef is not null && settings.Ef.SensitiveLogging;
        LogLevel globalLogLevel = settings.Ef is not null ? settings.Ef.LogLevel : LogLevel.Information;

        bool useInMemory = settings.Ef?.Sqlite?.UseInMemoryConnection ?? globalUseInMemory;

        bool sensitiveLogging = settings.Ef?.Sqlite?.SensitiveLogging ?? globalSensitiveLogging;

        LogLevel efLogLevel = settings.Ef?.Sqlite?.LogLevel ?? globalLogLevel;

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
                .LogTo(msg => System.Diagnostics.Debug.WriteLine(msg), efLogLevel);

            if (useInMemory)
            {
                SqliteConnection conn = provider.GetRequiredService<SqliteConnection>();
                options.UseSqlite(conn, options =>
                    options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            }
            else
            {
                string cs = settings.PlumbingConnectionString;
                string dataSource = new SqliteConnectionStringBuilder(cs).DataSource;

                Directory.CreateDirectory(Path.GetDirectoryName(dataSource)!);
                options.UseSqlite(cs, sqlOptions =>
                    sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            }
        });
        #endregion

        #region Register Repositories
        services.AddScoped<IRepository<CaliperEntity>, CaliperRepository>();
        services.AddScoped<IRepository<CornCaliperLink>, CornCaliperLinkRepository>();
        services.AddScoped<IRepository<CornPlumbingLink>, CornPlumbingLinkRepository>();
        services.AddScoped<IRepository<CornEntity>, CornRepository>();
        services.AddScoped<IRepository<CustardCaliperLink>, CustardCaliperLinkRepository>();
        services.AddScoped<IRepository<CustardCornLink>, CustardCornLinkRepository>();
        services.AddScoped<IRepository<CustardPlumbingLink>, CustardPlumbingLinkRepository>();
        services.AddScoped<IRepository<CustardEntity>, CustardRepository>();
        services.AddScoped<IRepository<PlumbingCaliperLink>, PlumbingCaliperLinkRepository>();
        services.AddScoped<IRepository<PlumbingEntity>, PlumbingRepository>();
        services.AddScoped<IRepository<PlumbingPhoneNumber>, PlumbingPhoneNumbersRepository>();
        services.AddScoped<IRepository<SandCaliperLink>, SandCaliperLinkRepository>();
        services.AddScoped<IRepository<SandCornLink>, SandCornLinkRepository>();
        services.AddScoped<IRepository<SandPlumbingLink>, SandPlumbingLinkRepository>();
        services.AddScoped<IRepository<SandEntity>, SandRepository>();
        services.AddScoped<ISyncStateRepository, SyncStateRepository>();
        services.AddScoped<ISyncStampRepository, SyncStampRepository>();
        services.AddScoped<IOAuthTokenRepository, OAuthTokenRepository>();

        services.AddScoped<IRepositoryFactory, RepositoryFactory>();

        #endregion

        return services;
    }

}
