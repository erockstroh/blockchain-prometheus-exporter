using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Prometheus;

namespace Blockchain.Prometheus.Exporter
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // set up basic retry durations
            // retry twice after 30 seconds each
            TimeSpan[ ] retryDurations =
            {
                TimeSpan.FromSeconds(30),
                TimeSpan.FromSeconds(30)
            };

            string rpcUrl = _configuration["RPC_URL"];

            services.AddHttpClient("rpc", client =>
                     {
                         client.BaseAddress = new Uri(rpcUrl);
                         client.Timeout = TimeSpan.FromSeconds(20);
                     })
                    .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(retryDurations));
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            app.UseHttpMetrics();
            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapMetrics(); });
        }
    }
}
