using Microsoft.Extensions.DependencyInjection;
using LeadPipe.Application.Service;
using LeadPipe.Infrastructure.Settings;
using LeadPipe.Infrastructure.Service;
using LeadPipe.Infrastructure.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure;

public static class InjectInfrastructure
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IInfrastructureSettings settings, IConfiguration configuration)
    {
        // Format: services.AddScoped<Interface, Class>();
        services.AddScoped<ICsvRwService, CsvRwService>();
        services.AddScoped<IFileConversionService, FileConversionService>();
        services.AddScoped<IJsonRwService, JsonRwService>();
        services.AddScoped<ILeafClientService, LeafClientService>();
        services.AddScoped<IYellerClientService, YellerClientService>();

        // Add Leaf Client
        services.AddHttpClient(settings.LeafName!, c =>
        {
            c.BaseAddress = new Uri(settings.LeafBase!);
            c.DefaultRequestHeaders.Add("Accept", "application/json");
            c.DefaultRequestHeaders.Add("Authorization", settings.LeafTokenType);
        });

        services.AddHttpClient();

        // Add Databases
        services.AddDbContext<PlumbingContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("PlumbingContext");
            options.UseSqlite(connectionString);
        });

        return services;
    }
}
