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
        // Register MySqlContext for Schema1
        if (string.IsNullOrWhiteSpace(settings.Schema1ConnectionString))
            throw new InvalidOperationException("MySqlConnectionString for Schema1 is missing.");

        services.AddDbContext<MySqlSchema1Context>((sp, options) =>
        {
            options.UseMySql(
                settings.Schema1ConnectionString,
                ServerVersion.AutoDetect(settings.Schema1ConnectionString),
                mySqlOptions =>
                {
                    mySqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                })
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging();
        });

        // Register MySqlContext for Schema2 
        if (string.IsNullOrWhiteSpace(settings.Schema2ConnectionString))
            throw new InvalidOperationException("MySqlConnectionString for Schema2 is missing.");

        services.AddDbContext<MySqlSchema2Context>((serviceProvider, options) =>
        {
            options.UseMySql(
                settings.Schema2ConnectionString,
                ServerVersion.AutoDetect(settings.Schema2ConnectionString),
                mySqlOptions =>
                {
                    mySqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                })
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging();
        });

        // Register MySqlContext for Schema3
        if (string.IsNullOrWhiteSpace(settings.Schema3ConnectionString))
            throw new InvalidOperationException("MySqlConnectionString for Schema3 is missing.");

        services.AddDbContext<MySqlSchema3Context>(options =>
        {
            options.UseMySql(
                settings.Schema3ConnectionString,
                ServerVersion.AutoDetect(settings.Schema3ConnectionString),
                mySqlOptions =>
                {
                    mySqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                })
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging();
        });


        // Register repositories
        services.AddScoped<ICaliperMySqlRepository, CaliperMySqlRepository>();
        services.AddScoped<ICustomerMySqlRepository, CustomerMySqlRepository>();
        services.AddScoped<ISubMySqlRepository, SubMySqlRepository>();
        services.AddScoped<ISummaryMySqlRepository, SummaryMySqlRepository>();

        return services;
    }
}
