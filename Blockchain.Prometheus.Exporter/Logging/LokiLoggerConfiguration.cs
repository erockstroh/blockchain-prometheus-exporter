using System;
using Blockchain.Prometheus.Exporter.Models.Enums;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Blockchain.Prometheus.Exporter.Logging
{
    /// <summary>
    /// Configurations for Loki logger
    /// </summary>
    public class LokiLoggerConfiguration
    {
        private const string DefaultServiceName = "service";

        /// <summary>
        /// Minimum log level
        /// </summary>
        [UsedImplicitly]
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Service name to report
        /// </summary>
        [UsedImplicitly]
        public string ServiceName { get; set; }

        /// <summary>
        /// Colour mode that should be used.
        /// </summary>
        [UsedImplicitly]
        public ColourMode ColourMode { get; set; } = ColourMode.Auto;

        /// <summary>
        /// Whether the service name should be written.
        /// </summary>
        [UsedImplicitly]
        public bool WriteServiceName { get; set; }

        /// <summary>
        /// Creates a new <see cref="LokiLoggerConfiguration"/> instance.
        /// </summary>
        /// <param name="configuration">Configuration containing logger settings information.</param>
        public LokiLoggerConfiguration([NotNull] IConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            IConfigurationSection section = configuration.GetSection("Logging:Loki");

            // allow overwriting of some values
            section?.Bind(this);

            // try getting log level from configuration
            string configLogLevel = configuration.GetValue<string>("Logging:Loki:LogLevel")
                                 ?? configuration.GetValue<string>("Logging:LogLevel:Default")
                                 ?? "Information";

            if (!Enum.TryParse(configLogLevel, true, out LogLevel logLevel))
            {
                logLevel = LogLevel.Information;
            }

            LogLevel = logLevel;

            if (string.IsNullOrWhiteSpace(ServiceName))
            {
                ServiceName = DefaultServiceName;
            }

            // make sure we detect the colour mode only once
            if (ColourMode != ColourMode.Auto)
            {
                return;
            }

            // detect whether we have an XTerm
            bool isXTermBash = configuration.GetValue<string>("TERM")?.Contains("xterm") ?? false;

            ColourMode = isXTermBash ? ColourMode.XTerm : ColourMode.Console;
        }
    }
}
