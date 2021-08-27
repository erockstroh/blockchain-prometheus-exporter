using System.Threading.Tasks;
using Blockchain.Prometheus.Exporter.Models;
using Blockchain.Prometheus.Exporter.Services;
using Blockchain.Prometheus.Exporter.Tests.Models.TestCases.Services;
using Blockchain.Prometheus.Exporter.Tests.TestCases.Services;
using FluentAssertions;
using Xunit;

namespace Blockchain.Prometheus.Exporter.Tests.Services
{
    public class CodeMonkeyTests
    {
        /// <summary>
        /// Test successful extraction of node versions.
        /// </summary>
        /// <param name="testCase">Test case for testing node version extraction.</param>
        [Theory]
        [MemberData(nameof(ExtractNodeVersionTestCases.ValidTestCases), MemberType = typeof(ExtractNodeVersionTestCases))]
        public Task TestExtractNodeVersion(ExtractNodeVersionTestCase testCase)
        {
            NodeClientVersion output = CodeMonkey.ExtractNodeVersion(testCase.Input);

            output.Should().NotBeNull();
            output.Should().BeEquivalentTo(testCase.Expected);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Tests that <see cref="CodeMonkey.ExtractNodeVersion"/> returns null if the version string is invalid.
        /// </summary>
        /// <param name="input">Input for test case.</param>
        [Theory]
        [InlineData("some-invalid/version")]
        [InlineData("invalid/1.1.0/missing-language")]
        [InlineData("")]
        [InlineData("something-arbitrary")]
        [InlineData("foo/v1.1.0/too-many//slashes")]
        [InlineData("foo/v1.1.0//too-many/slashes2.0")]
        public Task TestExtractNodeVersionNull(string input)
        {
            NodeClientVersion output = CodeMonkey.ExtractNodeVersion(input);

            output.Should().BeNull();

            return Task.CompletedTask;
        }
    }
}
