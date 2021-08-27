using System.Text.RegularExpressions;
using Blockchain.Prometheus.Exporter.Models;

namespace Blockchain.Prometheus.Exporter.Services
{
    public static class CodeMonkey
    {
        /// <summary>
        /// Regular expression for parsing client versions.
        /// </summary>
        private static readonly Regex VersionRegex = new(
            @"^(?<client>[^/]+)//?(?<version>(?<short>v?\d+\.\d+\.\d+)[^/]*)/(?<arch>[^/]+)/(?<lang>[^/$]+)$",
            RegexOptions.Compiled);

        public static NodeClientVersion ExtractNodeVersion(string clientVersion)
        {
            Match match = VersionRegex.Match(clientVersion);

            if (!match.Success)
            {
                return null;
            }

            string client = match.Groups["client"].Value;
            string version = match.Groups["version"].Value;
            string shortVersion = match.Groups["short"].Value;
            string architecture = match.Groups["arch"].Value;
            string language = match.Groups["lang"].Value;

            return new NodeClientVersion
            {
                Client = client,
                Version = version,
                ShortVersion = shortVersion,
                Architecture = architecture,
                Language = language
            };
        }
    }
}
