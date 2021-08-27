using System.Collections.Generic;
using Blockchain.Prometheus.Exporter.Models;
using Blockchain.Prometheus.Exporter.Services;
using Blockchain.Prometheus.Exporter.Tests.Models.TestCases.Services;
using Blockchain.Prometheus.Exporter.Tests.Shared;

namespace Blockchain.Prometheus.Exporter.Tests.TestCases.Services
{
    /// <summary>
    /// Test cases for testing <see cref="CodeMonkey.ExtractNodeVersion"/>.
    /// </summary>
    public class ExtractNodeVersionTestCases
    {
        /// <summary>
        /// Defines valid test cases that should pass.
        /// </summary>
        public static IEnumerable<object[ ]> ValidTestCases()
        {
            // test typical Erigon client version
            yield return Create("erigon/2021.07.4/linux-amd64/go1.16.6").TestCase(testCase =>
            {
                testCase.Expected = new NodeClientVersion
                {
                    Architecture = "linux-amd64",
                    Client = "erigon",
                    Language = "go1.16.6",
                    Version = "2021.07.4",
                    ShortVersion = "2021.07.4"
                };
            });

            // test typical Geth client version
            yield return Create("Geth/v1.10.8-stable-26675454/linux-amd64/go1.16.7").TestCase(testCase =>
            {
                testCase.Expected = new NodeClientVersion
                {
                    Architecture = "linux-amd64",
                    Client = "Geth",
                    Language = "go1.16.7",
                    Version = "v1.10.8-stable-26675454",
                    ShortVersion = "v1.10.8"
                };
            });

            // test typical Nethermind client version
            yield return Create("Nethermind/v1.10.79-0-e45db5fb5-20210826/X64-Linux/5.0.8").TestCase(
                testCase =>
                {
                    testCase.Expected = new NodeClientVersion
                    {
                        Architecture = "X64-Linux",
                        Client = "Nethermind",
                        Language = "5.0.8",
                        Version = "v1.10.79-0-e45db5fb5-20210826",
                        ShortVersion = "v1.10.79"
                    };
                });

            // test typical OpenEthereum client version
            yield return Create("OpenEthereum//v3.3.0-rc.4-stable/x86_64-linux-musl/rustc1.47.0").TestCase(
                testCase =>
                {
                    testCase.Expected = new NodeClientVersion
                    {
                        Architecture = "x86_64-linux-musl",
                        Client = "OpenEthereum",
                        Language = "rustc1.47.0",
                        Version = "v3.3.0-rc.4-stable",
                        ShortVersion = "v3.3.0"
                    };
                });

            static ExtractNodeVersionTestCase Create(string input)
            {
                return new ExtractNodeVersionTestCase
                {
                    Input = input
                };
            }
        }
    }
}
