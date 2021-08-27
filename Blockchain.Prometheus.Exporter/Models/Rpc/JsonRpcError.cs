using Newtonsoft.Json;

namespace Blockchain.Prometheus.Exporter.Models.Rpc
{
    /// <summary>
    /// Represents an error object that has been returned by an RPC client call.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonRpcError
    {
        /// <summary>
        /// Error code.
        /// </summary>
        [JsonProperty("code")]
        public int Code { get; set; }

        /// <summary>
        /// Message describing the error.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
