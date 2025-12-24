using LeadPipe.Application.Manager;
using LeadPipe.Application.UpdateReportPipeline;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LeadPipe.Application;

public static class InjectApplication
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Format: services.AddScoped<Interface, Class>();  

        // Add Update managers
        services.AddScoped<IUpdateCalliManager, UpdateCalliManager>();
        services.AddScoped<IUpdateLabManager, UpdateLabManager>();
        services.AddScoped<IUpdateLeafManager, UpdateLeafManager>();
        services.AddScoped<IUpdateLeasedManager, UpdateLeasedManager>();
        services.AddScoped<IUpdateLibacionManager, UpdateLibacionManager>();
        services.AddScoped<IUpdatePanManager, UpdatePanManager>();
        services.AddScoped<IUpdateYellerManager, UpdateYellerManager>();
        services.AddScoped<IUpdateCallsManager, UpdateCallsManager>();
        services.AddScoped<IUpdateSandwichManager, UpdateSandwichManager>();

        // Add Report managers
        services.AddScoped<IReportCalliManager, ReportCalliManager>();
        services.AddScoped<IReportLabManager, ReportLabManager>();
        services.AddScoped<IReportLeafManager, ReportLeafManager>();
        services.AddScoped<IReportLeasedManager, ReportLeasedManager>();
        services.AddScoped<IReportLibacionManager, ReportLibacionManager>();
        services.AddScoped<IReportPanManager, ReportPanManager>();
        services.AddScoped<IReportYellerManager, ReportYellerManager>();

        // Add managers
        services.AddScoped<IFileRWManager, FileRWManager>();
        services.AddScoped<IPlumbingAssociationManager, PlumbingAssociationManager>();

        return services;
    }
    private static void Register(IServiceCollection services, Type type)
    {
        Assembly? assembly = Assembly.GetAssembly(typeof(InjectApplication));
        if (assembly is null)
            return;

        IEnumerable<Type> managers = assembly
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                type.IsAssignableFrom(t)
            );

        foreach (Type? managerType in managers)
        {
            // Find the specific interface
            Type? serviceInterface = managerType
                .GetInterfaces()
                .FirstOrDefault(i =>
                    i != type &&
                    type.IsAssignableFrom(i));

            if (serviceInterface is null)
                continue;

            services.AddScoped(serviceInterface, managerType);
        }
    }
    private static void RegisterGeneric(IServiceCollection services, Type type)
    {
        Assembly? assembly = Assembly.GetAssembly(typeof(InjectApplication));
        if (assembly is null)
            return;

        IEnumerable<Type> managers = assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract);

        foreach (Type managerType in managers)
        {
            Type? serviceInterface = managerType
                .GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == type
                );
            if (serviceInterface is null)
                continue;

            services.AddScoped(serviceInterface, managerType);
        }
    }
}
