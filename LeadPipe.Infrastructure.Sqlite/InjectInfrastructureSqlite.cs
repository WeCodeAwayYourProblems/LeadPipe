using LeadPipe.Infrastructure.Entity.Sqlite;
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
                $"{nameof(settings.PlumbingConnectionString)} is not configured.");

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
