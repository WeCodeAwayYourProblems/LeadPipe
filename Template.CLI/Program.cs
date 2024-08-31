using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using CommandLine;
using Template.CLI.Verbs;
using Microsoft.Extensions.Hosting;

namespace Template.CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IHostBuilder builder = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.ConfigureCli(context.Configuration);
                });
            IHost host = builder.Build();
            IServiceProvider service = host.Services.CreateScope().ServiceProvider;
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
                _ => throw new Exception(),
            };
        }

        private static Type[] LoadVerbs()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetInterfaces().Contains(typeof(IVerb)))
                .Where(t => t.IsClass)
                .ToArray();
        }

        private static int HandleError(object o)
        {
            return o switch
            {
                IEnumerable<Error> e => Error(e),
                _ => ObjectError(o),
            };

            static int Error(IEnumerable<Error> e)
            {
                e.ToList().ForEach(r => System.Console.WriteLine(r.Tag));
                return ProgramErrorCodes.Error;
            }

            static int ObjectError(object o)
            {
                System.Console.WriteLine(o.ToString());
                return ProgramErrorCodes.Error;
            }
        }
        #endregion
    }
}
