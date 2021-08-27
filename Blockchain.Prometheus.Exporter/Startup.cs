using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blockchain.Prometheus.Exporter.Exceptions;
using Blockchain.Prometheus.Exporter.Extensions;
using Blockchain.Prometheus.Exporter.Models.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Prometheus;

namespace Blockchain.Prometheus.Exporter
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        private readonly ILogger<Startup> _logger;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            _configuration = configuration;
            _logger = logger;
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
                    .AddTransientHttpErrorPolicy(builder =>
                         builder.WaitAndRetryAsync(retryDurations, LogWarnOnRetry()));

            string rpcTargetValue = _configuration["RPC_TARGET"];

            if (!Enum.TryParse(rpcTargetValue, out RpcTarget rpcTarget))
            {
                throw new UnknownRpcTargetException(rpcTargetValue);
            }

            switch (rpcTarget)
            {
                case RpcTarget.Ethereum:
                    services.RegisterEthereumMetrics();

                    break;

                default:
                    throw new UnknownRpcTargetException(rpcTarget.ToString());
            }

            Func<DelegateResult<HttpResponseMessage>, TimeSpan, Task> LogWarnOnRetry()
            {
                return (_, _) =>
                {
                    _logger.LogWarning($"Retry connection to {rpcUrl}.");

                    return Task.CompletedTask;
                };
            }
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            app.UseHttpMetrics();
            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapMetrics(); });
        }
    }
}
