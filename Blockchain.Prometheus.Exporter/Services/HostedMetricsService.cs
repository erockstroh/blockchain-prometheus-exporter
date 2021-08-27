using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blockchain.Prometheus.Exporter.Models.Rpc;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Blockchain.Prometheus.Exporter.Services
{
    /// <summary>
    /// An <see cref="IHostedService"/> that collects metrics.
    /// </summary>
    public abstract class HostedMetricsService : IHostedService
    {
        /// <summary>
        /// Internal task that is being hosted.
        /// </summary>
        private Task _task;

        /// <summary>
        /// Current logger instance that can be used to write log messages.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Preferred timeout for waiting between collection tasks.
        /// </summary>
        protected abstract TimeSpan TaskTimeout { get; }

        /// <summary>
        /// Preferred timeout that the service waits before restarting.
        /// </summary>
        protected virtual TimeSpan RestartTimeout { get; }

        /// <summary>
        /// Factory for getting HTTP clients.
        /// </summary>
        private readonly IHttpClientFactory _httpFactory;

        /// <summary>
        /// Creates a new <see cref="HostedMetricsService"/> instance.
        /// </summary>
        /// <param name="logger">Logger that should be used.</param>
        /// <param name="httpFactory"><see cref="IHttpClientFactory"/> that should be used.</param>
        protected HostedMetricsService(ILogger logger, IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;

            Logger = logger;
            RestartTimeout = TimeSpan.FromSeconds(10);
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        /// <footer><a href="https://docs.microsoft.com/en-us/dotnet/api/Microsoft.Extensions.Hosting.IHostedService.StartAsync?view=netstandard-2.1">`IHostedService.StartAsync` on docs.microsoft.com</a></footer>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // dispose old task if any
            _task?.Dispose();

            // run task with default cancellation token
            _task = Task.Run(async () => await RunMetricsTask(cancellationToken), cancellationToken);

            // make sure task is restarted if something happens
            // do not restart task if it has been cancelled
            _task.ContinueWith(async task => await RestartMetricsTask(task, cancellationToken),
                cancellationToken, TaskContinuationOptions.NotOnCanceled, TaskScheduler.Current);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs a metrics task.
        /// </summary>
        /// <param name="cancellationToken">Current task cancellation token.</param>
        private async Task RunMetricsTask(CancellationToken cancellationToken)
        {
            try
            {
                // check if task has already been cancelled
                cancellationToken.ThrowIfCancellationRequested();

                do
                {
                    await CollectMetrics(cancellationToken);

                    // check if task has been cancelled
                    cancellationToken.ThrowIfCancellationRequested();

                    // delay task for specified amount of time
                    await Task.Delay(TaskTimeout, cancellationToken);
                } while (true);
            }
            catch (OperationCanceledException)
            {
                // throw cancellation operation upwards
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, $"Error during {nameof(RunMetricsTask)} execution.");
            }
        }

        /// <summary>
        /// Restart metrics task if it stopped unexpectedly.
        /// </summary>
        /// <param name="task">Task that terminated.</param>
        /// <param name="cancellationToken">Current task cancellation token.</param>
        private async Task RestartMetricsTask(Task task, CancellationToken cancellationToken)
        {
            try
            {
                if (task.IsCanceled)
                {
                    // return if task has been cancelled
                    return;
                }

                // check if task has been cancelled
                cancellationToken.ThrowIfCancellationRequested();

                // wait a bit before restarting
                await Task.Delay(RestartTimeout, cancellationToken);

                // check if task has been cancelled
                cancellationToken.ThrowIfCancellationRequested();

                // restart metrics task
                await StartAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // throw cancellation operation upwards
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, $"Error during {nameof(RestartMetricsTask)} execution.");
            }
        }

        /// <summary>
        /// Collect metrics from host.
        /// </summary>
        /// <param name="cancellationToken">Current task cancellation token.</param>
        /// <returns>Task that can be waited on.</returns>
        protected abstract Task CollectMetrics(CancellationToken cancellationToken);

        protected Task<IEnumerable<JsonRpcResponse>> SendRequest([NotNull] params JsonRpcRequest[ ] requests)
        {
            if (requests == null)
            {
                throw new ArgumentNullException(nameof(requests));
            }

            return SendRequest(send => send(), requests);
        }

        /// <summary>
        /// Sends a request to the RPC client.
        /// </summary>
        /// <param name="wrapSend">Wrapper method that wraps the actual send method.</param>
        /// <param name="requests">Requests that should be sent to the RPC client.</param>
        /// <returns>Responses for all requests.</returns>
        /// <exception cref="ArgumentNullException">Thrown if no requests are defined.</exception>
        /// <remarks>The return order of the responses might be different from the request order.</remarks>
        protected async Task<IEnumerable<JsonRpcResponse>> SendRequest(
            Func<Func<Task<HttpResponseMessage>>, Task<HttpResponseMessage>> wrapSend,
            [NotNull] params JsonRpcRequest[ ] requests)
        {
            if (requests == null)
            {
                throw new ArgumentNullException(nameof(requests));
            }

            string message = JsonConvert.SerializeObject(requests);

            HttpClient client = _httpFactory.CreateClient("rpc");

            Logger.LogInformation("Sending request to RPC upstream.");

            string responseBody;

            try
            {
                HttpResponseMessage responseMessage = await wrapSend(Send);

                responseBody = await responseMessage.Content.ReadAsStringAsync();
            }
            catch
            {
                Logger.LogError("FATAL: Upstream unreachable.");

                return default;
            }

            IEnumerable<JsonRpcResponse> response;

            try
            {
                response = JsonConvert.DeserializeObject<IEnumerable<JsonRpcResponse>>(responseBody);
            }
            catch
            {
                Logger.LogError($"FATAL: Error converting RPC result {responseBody} to " +
                                $"{typeof(IEnumerable<JsonRpcResponse>)}");

                response = default;
            }

            return response;

            Task<HttpResponseMessage> Send()
            {
                return client.PostAsync("", new StringContent(message, Encoding.UTF8, "application/json"));
            }
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        /// <footer><a href="https://docs.microsoft.com/en-us/dotnet/api/Microsoft.Extensions.Hosting.IHostedService.StopAsync?view=netstandard-2.1">`IHostedService.StopAsync` on docs.microsoft.com</a></footer>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            // force task to exit after 5 seconds
            _task?.Wait(TimeSpan.FromSeconds(5));

            // dispose of task
            _task?.Dispose();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the RPC result with the specified identifier as a string.
        /// </summary>
        /// <param name="responses">Set of RPC responses.</param>
        /// <param name="id">Indicates the identifier that was used to send an RPC request.</param>
        /// <returns>Result of the response as a string, or null if either the response was not found or the
        /// there is no result inside the response.</returns>
        protected Task<string> GetStringRpcResult(IEnumerable<JsonRpcResponse> responses, int id)
        {
            JsonRpcResponse response = responses.FirstOrDefault(r => r.Id == id);

            return Task.FromResult(response?.Result?.ToString());
        }

        /// <summary>
        /// Tries to parse a value as a hex value.
        /// </summary>
        /// <param name="value">Value that should be parsed.</param>
        /// <param name="result">Integer representing the hex value, or -1 if value is not in valid hex format.</param>
        /// <returns>Whether the <paramref name="value"/> represents a valid hex number.</returns>
        protected static bool TryParseHex(string value, out int result)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return int.TryParse(value.Replace("0x", ""),
                    NumberStyles.HexNumber, CultureInfo.InvariantCulture,
                    out result);
            }

            result = -1;

            return false;
        }
    }
}
