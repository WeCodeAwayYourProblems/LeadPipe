using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LeadPipe.Application;
using LeadPipe.Translation;
using LeadPipe.Infrastructure;
using LeadPipe.Infrastructure.MySql;
using LeadPipe.Infrastructure.Sqlite;

namespace LeadPipe.Cli;

internal static class ConfigureCommandLine
{
    public static void ConfigureCli(this IServiceCollection services, IConfiguration configuration)
    {
        Settings settings = new();
        configuration.Bind(settings);

        // ConnectionStrings
        settings.PlumbingConnectionString =
            configuration.GetConnectionString("Plumbing");

        settings.Schema2ConnectionString =
            configuration.GetConnectionString("Schema2");

        settings.Schema3ConnectionString =
            configuration.GetConnectionString("Schema3");

        settings.SchemaConnectionString =
            configuration.GetConnectionString("Schema");

        // Tokens
        configuration.GetSection("LabToken").Bind(settings.LabToken);

        configuration.GetSection("YellerToken").Bind(settings.YellerToken);

        configuration.GetSection("LeafToken").Bind(settings.LeafToken);

        // register once
        services.AddSingleton(settings);

        // expose via interfaces
        typeof(Settings)
            .GetInterfaces()
            .ToList()
            .ForEach(i => services.AddSingleton(i, settings));

        services.AddLogging(builder =>
        {
            builder.AddDebug();
            builder.AddConsole();
        });

        services
            .AddTranslation(settings)
            .AddInfrastructure(settings)
            .AddInfrastructureMySql(settings)
            .AddInfrastructureSqlite(settings)
            .AddApplication();
    }
}
