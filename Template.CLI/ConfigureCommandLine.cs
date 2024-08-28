using Microsoft.Extensions.DependencyInjection;
using Template.Application;
using Infrastructure;

namespace CLI;

internal static class ConfigureCommandLine
{
    public static void ConfigureCli(this IServiceCollection services)
    {
        services.AddInfrastructure().AddApplication();
    }
}
