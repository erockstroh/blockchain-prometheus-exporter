using Blockchain.Prometheus.Exporter.Logging;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Blockchain.Prometheus.Exporter
{
    public class Program
    {
        static void Main(params string[ ] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(params string[ ] args)
        {
            return WebHost
                  .CreateDefaultBuilder(args)
                  .ConfigureLogging((context, builder) =>
                   {
                       builder.ClearProviders();
                       builder.AddConfiguration(context.Configuration.GetSection("Logging"));

                       builder.AddProvider(new LokiLoggerProvider(new LokiLoggerConfiguration(context.Configuration)));
                   })
                  .UseStartup<Startup>()
                  .UseKestrel((_, options) => { options.ListenAnyIP(9368); });
        }
    }
}
