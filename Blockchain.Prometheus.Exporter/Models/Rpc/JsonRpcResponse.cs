using Newtonsoft.Json;

namespace Blockchain.Prometheus.Exporter.Models.Rpc
{
    /// <summary>
    /// Response of a JSON RPC request.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonRpcResponse
    {
        /// <summary>
        /// Version of JSON RPC that has been used.
        /// </summary>
        [JsonProperty("jsonrpc")]
        public string Version { get; set; }

        /// <summary>
        /// Value that has been returned.
        /// </summary>
        [JsonProperty("result")]
        public object Result { get; set; }

        /// <summary>
        /// Identifier of request.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Error that occurred when sending request.
        /// </summary>
        [JsonProperty("error")]
        public JsonRpcError Error { get; set; }
    }
}
