using LeadPipe.Infrastructure.Interfaces.Repository.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using LeadPipe.Infrastructure.MySql.Repository;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.MySql;

public static class InjectInfrastructureMySql
{
    public static IServiceCollection AddInfrastructureMySql(this IServiceCollection services, IMySqlSettings settings)
    {
        // Register MySqlContext
        if (string.IsNullOrWhiteSpace(settings.MySqlConnectionString))
            throw new InvalidOperationException("MySqlConnectionString is missing.");

        services.AddDbContext<MySqlContext>((serviceProvider, options) =>
        {
            options.UseMySql(
                settings.MySqlConnectionString,
                ServerVersion.AutoDetect(settings.MySqlConnectionString)
            )
            .LogTo(Console.WriteLine,LogLevel.Information)
            .EnableSensitiveDataLogging();
        });

        // Register repositories
        services.AddScoped<ICallMySqlRepository, CallMySqlRepository>();
        services.AddScoped<ICustomerMySqlRepository, CustomerMySqlRepository>();
        services.AddScoped<ISubMySqlRepository, SubMySqlRepository>();
        services.AddScoped<ISummaryMySqlRepository, SummaryMySqlRepository>();

        return services;
    }
}
