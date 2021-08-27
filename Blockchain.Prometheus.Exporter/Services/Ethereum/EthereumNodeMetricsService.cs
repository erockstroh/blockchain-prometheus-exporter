using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
        /// Regular expression for parsing client versions.
        /// </summary>
        private readonly Regex _versionRegex = new Regex(
            @"^(?<client>[^/]+)//?(?<version>(?<short>v?\d+\.\d+\.\d+)[^/]+)/(?<arch>[^/]+)/(?<lang>[^$]+)$",
            RegexOptions.Compiled);

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

            Match match = _versionRegex.Match(result);

            if (!match.Success)
            {
                return;
            }

            string client = match.Groups["client"].Value;
            string version = match.Groups["version"].Value;
            string shortVersion = match.Groups["short"].Value;
            string architecture = match.Groups["arch"].Value;
            string language = match.Groups["lang"].Value;

            EthereumMetricsCollection.NodeMetrics.NodeVersion.WithLabels(client, version, shortVersion,
                                          architecture, language)
                                     .Set(1);
        }
    }
}
