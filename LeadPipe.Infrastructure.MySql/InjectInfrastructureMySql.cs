using LeadPipe.Infrastructure.MySql.Context;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Infrastructure.MySql;

public static class InjectInfrastructureMySql
{
    public static IServiceCollection AddInfrastructureMySql(this IServiceCollection services, IMySqlSettings settings)
    {
        // Register MySqlContext
        services.AddDbContext<MySqlContext>((serviceProvider, options) =>
        {
            options.UseMySql(
                settings.MySqlConnectionString!,
                ServerVersion.AutoDetect(settings.MySqlConnectionString!)
            );
        });

        return services;
    }
}
