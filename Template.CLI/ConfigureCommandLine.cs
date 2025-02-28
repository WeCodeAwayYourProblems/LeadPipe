using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.Application;
using Template.Infrastructure;

namespace Template.CLI;

internal static class ConfigureCommandLine
{
    public static void ConfigureCli(this IServiceCollection services, IConfiguration configuration)
    {
        Settings settings = new();
        configuration.Bind(settings);

        typeof(Settings).GetInterfaces().ToList().ForEach(t => services.AddSingleton(t, settings));
        services.AddInfrastructure(settings).AddApplication();
    }
}
