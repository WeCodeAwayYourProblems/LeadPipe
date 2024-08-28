using Microsoft.Extensions.DependencyInjection;
using Template.Application;
using Template.Infrastructure;

namespace Template.CLI;

internal static class ConfigureCommandLine
{
    public static void ConfigureCli(this IServiceCollection services)
    {
        services.AddInfrastructure().AddApplication();
    }
}
