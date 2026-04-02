using LeadPipe.Application.Manager;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application;

public static class InjectApplication
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Format: services.AddScoped<Interface, Class>();  

        // Add managers
        services.AddScoped<IReportManager, ReportManager>();
        services.AddScoped<IUpdateManager, UpdateManager>();
        services.AddScoped<IFileRWManager, FileRWManager>();
        services.AddScoped<ICatManManager, CatManManager>();
        services.AddScoped<IAssociationManager, AssociationManager>();
        services.AddScoped<ICoreDataUpdateManager, CoreDataUpdateManager>();
        services.AddScoped<ISourceDataUpdateManager, SourceDataUpdateManager>();

        return services;
    }
}
