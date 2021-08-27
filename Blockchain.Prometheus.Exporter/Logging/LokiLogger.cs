using System;
using System.Text;
using System.Text.RegularExpressions;
using Blockchain.Prometheus.Exporter.Models.Enums;
using Microsoft.Extensions.Logging;

namespace Blockchain.Prometheus.Exporter.Logging
{
    /// <summary>
    /// Single line logger that works well with Loki.
    /// </summary>
    public class LokiLogger : ILogger
    {
        private readonly string _name;
        private readonly LokiLoggerConfiguration _config;

        private const string ColourPattern = @"&c(?<type>f|b):(?<colour>\w+);";
        private const string DataStart = "&[;";
        private const string DataEnd = "&];";
        private const string DataPattern = @"&\[;(?<data>.*?)&\];";
        private const string SwitchStart = "&&;";
        private const string SwitchEnd = ";&&";

        private readonly Regex _colourRegex = new Regex(ColourPattern);
        private readonly Regex _dataRegex = new Regex(DataPattern);
        private readonly Regex _switchRegex = new Regex($"{SwitchStart}|{SwitchEnd}");

        private static readonly object WriteLock = new object();

        /// <summary>
        /// Instantiate a new loki logger
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public LokiLogger(string name, LokiLoggerConfiguration config)
        {
            _name = name;
            _config = config;
        }

        /// <summary>Writes a log entry.</summary>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">Id of the event.</param>
        /// <param name="state">The entry to be written. Can be also an object.</param>
        /// <param name="exception">The exception related to this entry.</param>
        /// <param name="formatter">Function to create a <see cref="T:System.String" /> message of the <paramref name="state" /> and <paramref name="exception" />.</param>
        /// <typeparam name="TState">The type of the object to be written.</typeparam>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            StringBuilder outputBuilder = new StringBuilder();

            ConsoleColor defaultColor = Console.ForegroundColor;

            WriteColouredLevel(outputBuilder, logLevel, defaultColor);

            if (_config.WriteServiceName)
            {
                Write(outputBuilder, _config.ServiceName, null, ConsoleColor.Cyan);
                Write(outputBuilder, " - ", null, null);
            }

            Write(outputBuilder, formatter(state, exception), null, null);
            Write(outputBuilder, " - ", null, null);
            Write(outputBuilder, _name, null, ConsoleColor.DarkBlue);

            Flush(outputBuilder);
        }

        private void Flush(StringBuilder outputBuilder)
        {
            if (_config.ColourMode == ColourMode.None || _config.ColourMode == ColourMode.XTerm)
            {
                lock (WriteLock)
                {
                    Console.WriteLine(outputBuilder.ToString());
                }
            }
            else
            {
                lock (WriteLock)
                {
                    FlushToNonXTerm();
                }
            }

            outputBuilder.Clear();

            void FlushToNonXTerm()
            {
                string[ ] parts = _switchRegex.Split(outputBuilder.ToString());

                foreach (string part in parts)
                {
                    // check if we have colour matches on this part
                    foreach (Match match in _colourRegex.Matches(part))
                    {
                        bool isForeground = match.Groups["type"].Value.Equals("f");
                        string colourValue = match.Groups["colour"].Value;

                        if (!Enum.TryParse(colourValue, out ConsoleColor colour))
                        {
                            continue;
                        }

                        if (isForeground)
                        {
                            Console.ForegroundColor = colour;
                        }
                        else
                        {
                            Console.BackgroundColor = colour;
                        }
                    }

                    Match data = _dataRegex.Match(part);

                    Console.Write(data.Groups["data"].Value);

                    Console.ResetColor();
                }

                Console.WriteLine();
            }
        }

        private void WriteColouredLevel(StringBuilder outputBuilder, LogLevel level, ConsoleColor currentColour)
        {
            string levelName = "info";

            ConsoleColor colour = currentColour;

            switch (level)
            {
                case LogLevel.Trace:
                    levelName = "trac";
                    colour = ConsoleColor.Cyan;

                    break;

                case LogLevel.Debug:
                    levelName = "debg";
                    colour = ConsoleColor.Cyan;

                    break;

                case LogLevel.Information:
                    levelName = "info";
                    colour = ConsoleColor.Green;

                    break;

                case LogLevel.Warning:
                    levelName = "warn";
                    colour = ConsoleColor.Yellow;

                    break;

                case LogLevel.Error:
                    levelName = "erro";
                    colour = ConsoleColor.Red;

                    break;

                case LogLevel.Critical:
                    levelName = "crit";
                    colour = ConsoleColor.Magenta;

                    break;

                case LogLevel.None:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }

            Write(outputBuilder, $"{levelName}: ", null, colour);
        }

        /// <summary>
        /// Checks if the given <paramref name="logLevel" /> is enabled.
        /// </summary>
        /// <param name="logLevel">level to be checked.</param>
        /// <returns><c>true</c> if enabled.</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _config.LogLevel;
        }

        /// <summary>Begins a logical operation scope.</summary>
        /// <param name="state">The identifier for the scope.</param>
        /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
        /// <returns>An <see cref="T:System.IDisposable" /> that ends the logical operation scope on dispose.</returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        private void Write(StringBuilder outputBuilder, string message, ConsoleColor? background, ConsoleColor? foreground)
        {
            if (_config.ColourMode != ColourMode.None)
            {
                if (_config.ColourMode == ColourMode.Console)
                {
                    outputBuilder.Append(SwitchStart);
                }

                if (background.HasValue)
                {
                    outputBuilder.Append(GetBackgroundColorEscapeCode(background.Value));
                }

                if (foreground.HasValue)
                {
                    outputBuilder.Append(GetForegroundColorEscapeCode(foreground.Value));
                }

                if (_config.ColourMode == ColourMode.Console)
                {
                    outputBuilder.Append(DataStart);
                }
            }

            outputBuilder.Append(message);

            if (_config.ColourMode == ColourMode.None)
            {
                return;
            }

            if (_config.ColourMode == ColourMode.XTerm)
            {
                if (foreground.HasValue)
                {
                    outputBuilder.Append("\x001B[39m\x001B[22m");
                }

                if (background.HasValue)
                {
                    outputBuilder.Append("\x001B[49m");
                }
            }
            else
            {
                outputBuilder.Append(DataEnd);
                outputBuilder.Append(SwitchEnd);
            }
        }

        private string GetForegroundColorEscapeCode(ConsoleColor color)
        {
            if (_config.ColourMode == ColourMode.Console)
            {
                return NonXTermColour("f", color);
            }

            return color switch
            {
                ConsoleColor.Black       => "\x001B[30m",
                ConsoleColor.DarkBlue    => "\x001B[34m",
                ConsoleColor.DarkGreen   => "\x001B[32m",
                ConsoleColor.DarkCyan    => "\x001B[36m",
                ConsoleColor.DarkRed     => "\x001B[31m",
                ConsoleColor.DarkMagenta => "\x001B[35m",
                ConsoleColor.DarkYellow  => "\x001B[33m",
                ConsoleColor.Gray        => "\x001B[37m",
                ConsoleColor.Blue        => "\x001B[1m\x001B[34m",
                ConsoleColor.Green       => "\x001B[1m\x001B[32m",
                ConsoleColor.Cyan        => "\x001B[1m\x001B[36m",
                ConsoleColor.Red         => "\x001B[1m\x001B[31m",
                ConsoleColor.Magenta     => "\x001B[1m\x001B[35m",
                ConsoleColor.Yellow      => "\x001B[1m\x001B[33m",
                ConsoleColor.White       => "\x001B[1m\x001B[37m",
                _                        => "\x001B[39m\x001B[22m"
            };
        }

        private string GetBackgroundColorEscapeCode(ConsoleColor color)
        {
            if (_config.ColourMode == ColourMode.Console)
            {
                return NonXTermColour("b", color);
            }

            return color switch
            {
                ConsoleColor.Black   => "\x001B[40m",
                ConsoleColor.Blue    => "\x001B[44m",
                ConsoleColor.Green   => "\x001B[42m",
                ConsoleColor.Cyan    => "\x001B[46m",
                ConsoleColor.Red     => "\x001B[41m",
                ConsoleColor.Magenta => "\x001B[45m",
                ConsoleColor.Yellow  => "\x001B[43m",
                ConsoleColor.White   => "\x001B[47m",
                _                    => "\x001B[49m"
            };
        }

        private static string NonXTermColour(string type, ConsoleColor color)
        {
            return ColourPattern.Replace("(?<type>f|b)", type)
                                .Replace(@"(?<colour>\w+)", color.ToString());
        }
    }
}
