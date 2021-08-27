using Newtonsoft.Json;

namespace Blockchain.Prometheus.Exporter.Models.Rpc
{
    /// <summary>
    /// Synchronisation information for a node.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonRpcSyncInformation
    {
        /// <summary>
        /// Block the node is currently at.
        /// </summary>
        [JsonProperty("currentBlock")]
        public string CurrentBlock { get; set; }

        /// <summary>
        /// Block the node reports as highest block.
        /// </summary>
        [JsonProperty("highestBlock")]
        public string HighestBlock { get; set; }

        /// <summary>
        /// Block the synchronisation has started on.
        /// </summary>
        [JsonProperty("startingBlock")]
        public string StartingBlock { get; set; }
    }
}
