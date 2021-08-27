using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Blockchain.Prometheus.Exporter.Models.Ethereum;
using Blockchain.Prometheus.Exporter.Models.Rpc;
using Microsoft.Extensions.Logging;

namespace Blockchain.Prometheus.Exporter.Services.Ethereum
{
    /// <summary>
    /// Service that collections Ethereum node peer metrics.
    /// </summary>
    public class EthereumPeerMetricsService : HostedMetricsService
    {
        /// <summary>
        /// Preferred timeout for waiting between collection tasks.
        /// </summary>
        protected override TimeSpan TaskTimeout { get; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Creates a new <see cref="EthereumPeerMetricsService"/> instance.
        /// </summary>
        /// <param name="logger">Logger that should be used.</param>
        /// <param name="httpFactory"><see cref="IHttpClientFactory"/> that should be used.</param>
        public EthereumPeerMetricsService(ILogger<EthereumPeerMetricsService> logger, IHttpClientFactory httpFactory)
            : base(logger, httpFactory) { }

        /// <summary>
        /// Collect metrics from host.
        /// </summary>
        /// <param name="cancellationToken">Current task cancellation token.</param>
        /// <returns>Task that can be waited on.</returns>
        protected override async Task CollectMetrics(CancellationToken cancellationToken)
        {
            // check if we should have been cancelled
            cancellationToken.ThrowIfCancellationRequested();

            JsonRpcRequest request = new()
            {
                Id = 0x10,
                Method = "net_peerCount"
            };

            IEnumerable<JsonRpcResponse> responses = await SendRequest(request);

            string result = await GetStringRpcResult(responses, request.Id);

            if (TryParseHex(result, out int peers))
            {
                EthereumMetricsCollection.PeerMetrics.PeerCount.Set(peers);
            }
        }
    }
}
