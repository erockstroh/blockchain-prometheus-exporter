using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Blockchain.Prometheus.Exporter.Models;
using Blockchain.Prometheus.Exporter.Models.Ethereum;
using Blockchain.Prometheus.Exporter.Models.Rpc;
using Microsoft.Extensions.Logging;

namespace Blockchain.Prometheus.Exporter.Services.Ethereum
{
    /// <summary>
    /// Service that collections Ethereum node metrics.
    /// </summary>
    public class EthereumNodeMetricsService : HostedMetricsService
    {
        /// <summary>
        /// Preferred timeout for waiting between collection tasks.
        /// </summary>
        protected override TimeSpan TaskTimeout { get; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Creates a new <see cref="EthereumNodeMetricsService"/> instance.
        /// </summary>
        /// <param name="logger">Logger that should be used.</param>
        /// <param name="httpFactory"><see cref="IHttpClientFactory"/> that should be used.</param>
        public EthereumNodeMetricsService(ILogger<EthereumNodeMetricsService> logger, IHttpClientFactory httpFactory)
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
                Method = "web3_clientVersion"
            };

            IEnumerable<JsonRpcResponse> responses = await SendRequest(request);

            string result = await GetStringRpcResult(responses, request.Id);

            NodeClientVersion clientVersion = CodeMonkey.ExtractNodeVersion(result);

            EthereumMetricsCollection.NodeMetrics.NodeVersion.WithLabels(clientVersion.Client, clientVersion.Version,
                                          clientVersion.ShortVersion, clientVersion.Architecture,
                                          clientVersion.Language)
                                     .Set(1);
        }
    }
}
