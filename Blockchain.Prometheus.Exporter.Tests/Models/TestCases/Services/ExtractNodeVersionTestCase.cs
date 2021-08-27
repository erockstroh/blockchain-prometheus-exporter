using Blockchain.Prometheus.Exporter.Models;
using Blockchain.Prometheus.Exporter.Services;

namespace Blockchain.Prometheus.Exporter.Tests.Models.TestCases.Services
{
    /// <summary>
    /// Test case for testing <see cref="CodeMonkey.ExtractNodeVersion"/>.
    /// </summary>
    public class ExtractNodeVersionTestCase
    {
        /// <summary>
        /// Input for test case.
        /// </summary>
        public string Input { get; set; }

        /// <summary>
        /// Expected output of <see cref="CodeMonkey.ExtractNodeVersion"/> method call.
        /// </summary>
        public NodeClientVersion Expected { get; set; }
    }
}
