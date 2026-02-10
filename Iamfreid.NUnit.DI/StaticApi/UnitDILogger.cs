using System;
using Microsoft.Extensions.Logging;

namespace Iamfreid.NUnit.DI.StaticApi
{
    /// <summary>
    /// Provides logging functionality for UnitDI library.
    /// </summary>
    internal static class UnitDILogger
    {
        private static ILoggerFactory? _loggerFactory;
        private static ILogger? _defaultLogger;

        /// <summary>
        /// Sets the logger factory for UnitDI logging.
        /// </summary>
        /// <param name="loggerFactory">The logger factory to use. Pass null to disable logging.</param>
        public static void SetLoggerFactory(ILoggerFactory? loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Gets a logger instance for the specified category name.
        /// </summary>
        /// <param name="categoryName">The category name for the logger (e.g., class name).</param>
        /// <returns>
        /// A logger instance. If a logger factory is configured, returns a logger from that factory.
        /// Otherwise, returns a null logger (no-op).
        /// </returns>
        internal static ILogger GetLogger(string categoryName)
        {
            if (_loggerFactory != null)
            {
                return _loggerFactory.CreateLogger(categoryName);
            }

            if (_defaultLogger == null)
            {
                _defaultLogger = new NullLogger();
            }

            return _defaultLogger;
        }

        /// <summary>
        /// Gets a logger instance for the specified type.
        /// Uses the type's full name as the category name.
        /// </summary>
        /// <typeparam name="T">The type to use as the logger category.</typeparam>
        /// <returns>
        /// A logger instance. If a logger factory is configured, returns a logger from that factory.
        /// Otherwise, returns a null logger (no-op).
        /// </returns>
        internal static ILogger GetLogger<T>()
        {
            return GetLogger(typeof(T).FullName ?? typeof(T).Name);
        }

        /// <summary>
        /// A null logger implementation that performs no operations.
        /// Follows the Null Object pattern to provide a safe no-op implementation.
        /// </summary>
        private sealed class NullLogger : ILogger
        {
            /// <inheritdoc />
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            /// <inheritdoc />
            public bool IsEnabled(LogLevel logLevel) => false;

            /// <inheritdoc />
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                
            }
        }
    }
}

