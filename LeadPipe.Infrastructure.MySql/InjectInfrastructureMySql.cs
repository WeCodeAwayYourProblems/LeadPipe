using LeadPipe.Infrastructure.MySql.Context;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Infrastructure.MySql;

public static class InjectInfrastructureMySql
{
    public static IServiceCollection AddInfrastructureMySql(this IServiceCollection services, IMySqlSettings config)
    {
        // Format: services.AddScoped<Interface, Class>();

        // Repositories

        // Logging

        // Add databases
        services.AddDbContext<MySqlContext>(options =>
            options.UseMySql(
                config.MySqlConnectionString!,
                ServerVersion.AutoDetect(config.MySqlConnectionString!)
            ));

        return services;
    }
}
