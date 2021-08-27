using Prometheus;

namespace Blockchain.Prometheus.Exporter.Models.Ethereum
{
    /// <summary>
    /// Defines metrics for block handling.
    /// </summary>
    public class EthereumBlockMetrics
    {
        /// <summary>
        /// Tracks the current block number.
        /// </summary>
        public readonly Gauge BlockNumber = Metrics.CreateGauge
            ("eth_block_number", "Current block number");

        /// <summary>
        /// Tracks the current highest block number.
        /// </summary>
        public readonly Gauge HighestBlockNumber = Metrics.CreateGauge
            ("eth_highest_block_number", "Highest block number");

        /// <summary>
        /// Tracks the latency for the getting the latest block.
        /// </summary>
        public readonly Gauge BlockNumberLatency = Metrics.CreateGauge
            ("eth_block_number_latency", "Latency for getting latest block");

        /// <summary>
        /// Tracks the number of transactions for the latest block.
        /// </summary>
        public readonly Gauge LatestBlockTransactions = Metrics.CreateGauge
            ("eth_latest_block_transactions", "Latest block transaction count");

        /// <summary>
        /// Tracks whether the node is currently syncing.
        /// </summary>
        public readonly Gauge IsSyncing = Metrics.CreateGauge
            ("eth_syncing", "Whether node is currently syncing");
    }
}
