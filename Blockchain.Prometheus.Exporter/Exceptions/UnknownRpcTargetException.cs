using System;

namespace Blockchain.Prometheus.Exporter.Exceptions
{
    /// <summary>
    /// Thrown if the defined RPC target is not known.
    /// </summary>
    public class UnknownRpcTargetException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="UnknownRpcTargetException"/> instances.
        /// </summary>
        /// <param name="target">Target that is not known.</param>
        public UnknownRpcTargetException(string target) : base(GetDefaultMessage(target)) { }

        /// <summary>
        /// Returns the default exception message.
        /// </summary>
        /// <param name="target">Target that is not known.</param>
        /// <returns>Default error message.</returns>
        private static string GetDefaultMessage(string target)
        {
            return $"{target} is not a known RPC target.";
        }
    }
}
