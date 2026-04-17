using CommandLine;
using LeadPipe.Cli.Verbs;
using LeadPipe.Infrastructure.MySql.Context;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Reflection;

namespace LeadPipe.Cli;

internal class Program
{
    static void Main(string[] args)
    {
        IHostBuilder builder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                context.AddSecrets(config);
            })
            .UseSerilog((context, services, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration))
            .ConfigureServices((context, services) =>
            {
                services.ConfigureCli(context.Configuration);
            });
        IHost host = builder.Build();

        using IServiceScope scope = host.Services.CreateScope();
        IServiceProvider service = scope.ServiceProvider;
        IConfiguration config = service.GetRequiredService<IConfiguration>();
        var settings = service.GetRequiredService<Settings>();

        EnsureInMemoryDbIsCreated(service, settings);

        Execute(args, service);
    }

    #region Private Members
    private static ParserResult<object> Execute(string[] args, IServiceProvider service)
    {
        var types = LoadVerbs();
        var result = Parser.Default.ParseArguments(args, types)
            .WithParsed(obj => Run(obj, service))
            .WithNotParsed(o => HandleError(o));

        return result;
    }

    private static int Run(object obj, IServiceProvider service)
    {
        return obj switch
        {
            IVerb o => o.Run(service),
            IVerbAsync v => v.Run(service).GetAwaiter().GetResult(),
            _ => throw new Exception(),
        };
    }

    private static Type[] LoadVerbs() =>
        [
            .. Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => typeof(IVerb).IsAssignableFrom(t) || typeof(IVerbAsync).IsAssignableFrom(t))
                .Where(t => t.IsClass)
        ];

    private static int HandleError(object o)
    {
        return o switch
        {
            IEnumerable<Error> e => Error(e),
            _ => ObjectError(o),
        };

        static int Error(IEnumerable<Error> e)
        {
            e.ToList().ForEach(r => Console.WriteLine(r.Tag));
            return ProgramErrorCodes.Error;
        }

        static int ObjectError(object o)
        {
            Console.WriteLine(o.ToString());
            return ProgramErrorCodes.Error;
        }
    }

    private static void EnsureInMemoryDbIsCreated(IServiceProvider service, Settings settings)
    {
        // Make sure db is created if we're using inmemory
        bool sqliteInMemory = (bool)(settings.Ef?.Sqlite?.UseInMemoryConnection)!;
        bool mysqlInMemory = (bool)(settings.Ef?.MySql?.UseInMemoryDatabase)!;

        if (sqliteInMemory)
        {
            var pCtx = service.GetRequiredService<PlumbingContext>();
            pCtx.Database.EnsureCreated();
        }
        if (mysqlInMemory)
        {
            var mysql1Ctx = service.GetRequiredService<MySqlSchema1Context>();
            var mysql2Ctx = service.GetRequiredService<MySqlSchema2Context>();
            var mysql3Ctx = service.GetRequiredService<MySqlSchema3Context>();
            mysql1Ctx.Database.EnsureCreated();
            mysql2Ctx.Database.EnsureCreated();
            mysql3Ctx.Database.EnsureCreated();
        }
    }

    #endregion
}
