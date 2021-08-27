using Blockchain.Prometheus.Exporter.Services.Ethereum;
using Microsoft.Extensions.DependencyInjection;

namespace Blockchain.Prometheus.Exporter.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers metrics collection for Ethereum based clients.
        /// </summary>
        /// <param name="services">Current service collection.</param>
        /// <returns>Current service collection.</returns>
        public static IServiceCollection RegisterEthereumMetrics(this IServiceCollection services)
        {
            services.AddHostedService<EthereumBlockMetricsService>();
            services.AddHostedService<EthereumPeerMetricsService>();
            services.AddHostedService<EthereumNodeMetricsService>();

            return services;
        }
    }
}
