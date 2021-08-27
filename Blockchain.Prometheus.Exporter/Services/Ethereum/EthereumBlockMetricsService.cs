using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Blockchain.Prometheus.Exporter.Models.Ethereum;
using Blockchain.Prometheus.Exporter.Models.Rpc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Prometheus;

namespace Blockchain.Prometheus.Exporter.Services.Ethereum
{
    /// <summary>
    /// Service that collections Ethereum node block metrics.
    /// </summary>
    public class EthereumBlockMetricsService : HostedMetricsService
    {
        /// <summary>
        /// Preferred timeout for a running task.
        /// </summary>
        protected override TimeSpan TaskTimeout { get; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Creates a new <see cref="EthereumBlockMetricsService"/> instance.
        /// </summary>
        /// <param name="logger">Logger that should be used.</param>
        /// <param name="httpFactory"><see cref="IHttpClientFactory"/> that should be used.</param>
        public EthereumBlockMetricsService(ILogger<EthereumBlockMetricsService> logger, IHttpClientFactory httpFactory)
            : base(logger, httpFactory) { }

        /// <summary>
        /// Collect metrics from host.
        /// </summary>
        /// <param name="cancellationToken">Current task cancellation token.</param>
        /// <returns>Task that can be waited on.</returns>
        protected override async Task CollectMetrics(CancellationToken cancellationToken)
        {
            await CollectBasicMetrics(cancellationToken);

            await CollectLatencyMetrics(cancellationToken);
        }

        /// <summary>
        /// Collects basic node metrics.
        /// </summary>
        /// <param name="cancellationToken">Current task cancellation token.</param>
        private async Task CollectBasicMetrics(CancellationToken cancellationToken)
        {
            // check if we should have been cancelled
            cancellationToken.ThrowIfCancellationRequested();

            JsonRpcRequest blockNumber = new()
            {
                Method = "eth_blockNumber",
                Id = 0x10
            };

            JsonRpcRequest latestBlockTransactions = new()
            {
                Method = "eth_getBlockTransactionCountByNumber",
                Parameters = new object[] {"latest"},
                Id = 0x20
            };

            JsonRpcRequest isSyncing = new()
            {
                Method = "eth_syncing",
                Id = 0x30
            };

            IEnumerable<JsonRpcResponse> rpcResponses = await SendRequest(blockNumber,
                latestBlockTransactions, isSyncing);

            IEnumerable<JsonRpcResponse> responses = rpcResponses.ToList();

            string blockResult = await GetStringRpcResult(responses, blockNumber.Id);

            int highestBlock = int.MaxValue;

            if (TryParseHex(blockResult, out int block))
            {
                EthereumMetricsCollection.BlockMetrics.BlockNumber.Set(block);

                // use latest block for highest block number
                // this will be overwritten later if the node is syncing
                highestBlock = block;
            }

            string transactionResult = await GetStringRpcResult(responses, latestBlockTransactions.Id);

            if (TryParseHex(transactionResult, out int transactions))
            {
                EthereumMetricsCollection.BlockMetrics.LatestBlockTransactions.Set(transactions);
            }

            string isSyncingResult = await GetStringRpcResult(responses, isSyncing.Id);

            // RPC returns either a boolean if the node is not syncing,
            // or an Object if the node is syncing
            if (!bool.TryParse(isSyncingResult, out bool syncing))
            {
                // convert string to block information
                JsonRpcSyncInformation syncInformation =
                    JsonConvert.DeserializeObject<JsonRpcSyncInformation>(isSyncingResult);

                if (TryParseHex(syncInformation?.CurrentBlock, out int currentBlock) &&
                    TryParseHex(syncInformation?.HighestBlock, out highestBlock))
                {
                    // this is a workaround for Nethermind nodes
                    // apparently they still return the current and highest block even if they are fully synced
                    syncing = currentBlock != highestBlock;
                }
                else
                {
                    // assume node is syncing if a non-boolean is being returned
                    syncing = true;
                }
            }

            // update syncing state depending on value
            EthereumMetricsCollection.BlockMetrics.IsSyncing.Set(syncing ? 1 : 0);

            // update syncing information block metrics
            // ignore current block as it should be set by the initial metric
            // we are not here to fix other people's nodes if they report the wrong 'current' state
            EthereumMetricsCollection.BlockMetrics.HighestBlockNumber.Set(highestBlock);
        }

        /// <summary>
        /// Collects latency metrics for calling a node.
        /// </summary>
        /// <param name="cancellationToken">Current task cancellation token.</param>
        private async Task CollectLatencyMetrics(CancellationToken cancellationToken)
        {
            // check if we should have been cancelled
            cancellationToken.ThrowIfCancellationRequested();

            // use an `eth_getBlockByNumber` request that includes transaction information
            // for getting a more accurate latency representation
            JsonRpcRequest request = new()
            {
                Method = "eth_getBlockByNumber",
                Parameters = new object[] {"latest", true},
                Id = 0x42
            };

            // we are only interested in timing the RPC call
            // ignore result of function call
            await SendRequest(send =>
            {
                using (EthereumMetricsCollection.BlockMetrics.BlockNumberLatency.NewTimer())
                {
                    return send();
                }
            }, request);
        }
    }
}
