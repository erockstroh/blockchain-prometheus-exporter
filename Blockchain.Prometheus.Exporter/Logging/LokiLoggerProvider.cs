using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Blockchain.Prometheus.Exporter.Logging
{
    /// <summary>
    /// Log provider logging to a single line
    /// </summary>
    public class LokiLoggerProvider : ILoggerProvider
    {
        private readonly LokiLoggerConfiguration _lokiConfiguration;

        private readonly ConcurrentDictionary<string, LokiLogger> _loggers = new();

        /// <summary>
        /// Create new log provider
        /// </summary>
        /// <param name="lokiConfiguration">Loki logger configuration</param>
        public LokiLoggerProvider(LokiLoggerConfiguration lokiConfiguration)
        {
            _lokiConfiguration = lokiConfiguration;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <footer><a href="https://docs.microsoft.com/en-us/dotnet/api/System.IDisposable.Dispose?view=netcore-5.0">`IDisposable.Dispose` on docs.microsoft.com</a></footer>
        public void Dispose()
        {
            _loggers.Clear();
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Extensions.Logging.ILogger" /> instance.
        /// </summary>
        /// <param name="categoryName">The category name for messages produced by the logger.</param>
        /// <returns>The instance of <see cref="T:Microsoft.Extensions.Logging.ILogger" /> that was created.</returns>
        /// <footer><a href="https://docs.microsoft.com/en-us/dotnet/api/Microsoft.Extensions.Logging.ILoggerProvider.CreateLogger?view=netcore-5.0">`ILoggerProvider.CreateLogger` on docs.microsoft.com</a></footer>
        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new LokiLogger(name, _lokiConfiguration));
        }
    }}
