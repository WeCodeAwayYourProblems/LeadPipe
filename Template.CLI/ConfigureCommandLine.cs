using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Template.Application;
using Template.Infrastructure;

namespace Template.Cli;

internal static class ConfigureCommandLine
{
    public static void ConfigureCli(this IServiceCollection services, IConfiguration configuration)
    {
        Settings settings = new();
        configuration.Bind(settings);

        typeof(Settings).GetInterfaces().ToList().ForEach(t => services.AddSingleton(t, settings));
        services.AddLogging(builder =>
        {
            builder.AddDebug();
            builder.AddConsole();
        });
        services.AddInfrastructure(settings).AddApplication();
    }
}
