namespace Dashing.Console.Weaving {
    using System.Collections.Generic;

    using Dashing.Tools;

    public class NullLogger : ILogger {
        public void Trace(string message) {
        }

        public void Trace(string message, params object[] args) {
        }

        public void Trace<T>(IEnumerable<T> items, string[] columnHeaders = null) {
        }

        public void Error(string message) {
        }

        public void Error(string message, params object[] args) {
        }
    }
}