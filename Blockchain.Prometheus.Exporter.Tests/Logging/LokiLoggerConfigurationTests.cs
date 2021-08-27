using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Blockchain.Prometheus.Exporter.Logging;
using Blockchain.Prometheus.Exporter.Models.Enums;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Blockchain.Prometheus.Exporter.Tests.Logging
{
    public class LokiLoggerConfigurationTests
    {
        private static readonly LokiLoggerConfiguration DefaultConfiguration = new(BuildEmptyConfiguration());

        [Theory]
        [InlineData(LogLevel.Information, "My-Service")]
        [InlineData(LogLevel.Critical, "")]
        [InlineData(LogLevel.None, null)]
        [InlineData(LogLevel.Warning, "eth-exporter-number-42")]
        public async Task TestLokiConfiguration(LogLevel logLevel, string serviceName)
        {
            // create application settings WITH Loki section
            string appSettings = "{\"Logging\":{\"LogLevel\":{\"Default\":\"Debug\"},"
                               + $"\"Loki\":{{\"LogLevel\":\"{logLevel}\",\"ServiceName\":\"{serviceName}\"}}}}}}";

            IConfiguration configuration = await BuildConfiguration(appSettings);

            LokiLoggerConfiguration loggerConfiguration = new(configuration);

            loggerConfiguration.LogLevel.Should().Be(logLevel,
                "it should have been read from the app settings");

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                loggerConfiguration.ServiceName.Should().Be(DefaultConfiguration.ServiceName,
                    "it is null or empty in settings");
            }
            else
            {
                loggerConfiguration.ServiceName.Should().Be(serviceName,
                    "it should have been read from the app settings");
            }
        }

        [Theory]
        [InlineData("My-Service")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("eth-exporter-number-42")]
        public async Task TestLokiConfigurationWithoutLogLevel(string serviceName)
        {
            // create application settings WITH PARTIAL Loki section without log level
            string appSettings = "{\"Logging\":{\"LogLevel\":{\"Default\":\"Debug\"},"
                               + $"\"Loki\":{{\"ServiceName\":\"{serviceName}\"}}}}}}\n";

            IConfiguration configuration = await BuildConfiguration(appSettings);

            LokiLoggerConfiguration loggerConfiguration = new(configuration);

            loggerConfiguration.LogLevel.Should().Be(LogLevel.Debug,
                "it should have been read from the default app settings");

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                loggerConfiguration.ServiceName.Should().Be(DefaultConfiguration.ServiceName,
                    "it null or empty in settings");
            }
            else
            {
                loggerConfiguration.ServiceName.Should().Be(serviceName,
                    "it should have been read from the app settings");
            }
        }

        [Theory]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.None)]
        [InlineData(LogLevel.Warning)]
        public async Task TestLokiConfigurationWithoutServiceName(LogLevel logLevel)
        {
            // create application settings WITH PARTIAL Loki section without log level
            string appSettings = "{\"Logging\":{\"LogLevel\":{\"Default\":\"Debug\"},"
                               + $"\"Loki\":{{\"LogLevel\":\"{logLevel}\"}}}}}}\n";

            IConfiguration configuration = await BuildConfiguration(appSettings);

            LokiLoggerConfiguration loggerConfiguration = new(configuration);

            loggerConfiguration.LogLevel.Should().Be(logLevel,
                "it should have been read from the app settings");

            loggerConfiguration.ServiceName.Should().Be(DefaultConfiguration.ServiceName,
                "it is not defined in the app settings");
        }

        [Theory]
        [InlineData(LogLevel.Information, "My-Service")]
        [InlineData(LogLevel.Critical, "")]
        [InlineData(LogLevel.None, null)]
        [InlineData(LogLevel.Warning, "eth-exporter-number-42")]
        public async Task TestDefaultConfiguration(LogLevel logLevel, string serviceName)
        {
            // create application settings WITHOUT Loki section
            string appSettings = $"{{\"Logging\":{{\"LogLevel\": {{\"Default\":\"{logLevel}\"}}}}}}";

            IConfiguration configuration = await BuildConfiguration(appSettings);

            LokiLoggerConfiguration loggerConfiguration = new(configuration);

            loggerConfiguration.LogLevel.Should().Be(logLevel,
                "it should have been read from the app settings");

            loggerConfiguration.ServiceName.Should().NotBe(serviceName,
                "it is not present in the app settings");
        }

        [Theory]
        [InlineData("xterm-256colors", ColourMode.XTerm)]
        [InlineData("another-xterm", ColourMode.XTerm)]
        [InlineData("windows", ColourMode.Console)]
        [InlineData("terminal", ColourMode.Console)]
        [InlineData("", ColourMode.Console)]
        public async Task TestAutoColourMode(string term, ColourMode expectedMode)
        {
            IConfiguration configuration = await BuildConfiguration(addValues: builder =>
            {
                builder.AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "TERM", term }
                });

                return Task.CompletedTask;
            });

            configuration.GetValue<string>("TERM").Should().Be(term);

            LokiLoggerConfiguration lokiConfiguration = new(configuration);

            lokiConfiguration.ColourMode.Should().Be(expectedMode, $"terminal has been defined as {term}");
        }

        private static IConfiguration BuildEmptyConfiguration()
        {
            return new ConfigurationBuilder().Build();
        }

        private static async Task<IConfiguration> BuildConfiguration(string appSettings = null,
            Func<IConfigurationBuilder, Task> addValues = null)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();

            await using MemoryStream stream = new();

            await using StreamWriter writer = new(stream);

            if (!string.IsNullOrWhiteSpace(appSettings))
            {
                await writer.WriteAsync(appSettings);
                await writer.FlushAsync();

                stream.Position = 0;

                builder.AddJsonStream(stream);
            }

            if (addValues != null)
            {
                await addValues.Invoke(builder);
            }

            IConfiguration configuration = builder.Build();

            return configuration;
        }
    }
}
