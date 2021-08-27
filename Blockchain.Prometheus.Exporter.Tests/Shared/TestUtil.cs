using System;
using System.Diagnostics.CodeAnalysis;

namespace Blockchain.Prometheus.Exporter.Tests.Shared
{
    public static class TestUtil
    {
        /// <summary>
        /// Converts an object to an xUnit test case.
        /// </summary>
        /// <param name="testCase">Test case that should be converted.</param>
        /// <param name="setup">Callback for setting up test case further.</param>
        /// <typeparam name="TCase">Type of test case that should be converted.</typeparam>
        /// <returns>Object array that only contains the one test case.</returns>
        [SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global", Justification = "xUnit requires" +
            "this to be object[]")]
        public static object[] TestCase<TCase>(this TCase testCase, Action<TCase> setup = null)
        {
            setup?.Invoke(testCase);

            return new object[]
            {
                testCase
            };
        }
    }
}
