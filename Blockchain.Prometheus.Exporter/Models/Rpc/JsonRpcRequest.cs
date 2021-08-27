using System.Collections.Generic;
using Newtonsoft.Json;

namespace Blockchain.Prometheus.Exporter.Models.Rpc
{
    /// <summary>
    /// Represents a request that can be sent to an RPC client.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonRpcRequest
    {
        /// <summary>
        /// Method that should be called.
        /// </summary>
        [JsonProperty("method", Order = 0)]
        public string Method { get; set; }

        /// <summary>
        /// Additional parameters for request.
        /// </summary>
        [JsonProperty("params", Order = 1)]
        public IEnumerable<object> Parameters { get; set; } = new List<object>();

        /// <summary>
        /// Request identifier.
        /// </summary>
        [JsonProperty("id", Order = 2)]
        public int Id { get; set; }

        /// <summary>
        /// Version of Json RPC that is being used.
        /// </summary>
        [JsonProperty("jsonrpc")]
        public string Version { get; } = "2.0";
    }
}
