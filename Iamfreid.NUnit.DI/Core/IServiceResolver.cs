using System;

namespace Iamfreid.NUnit.DI.Core
{
    /// <summary>
    /// Provides service resolution functionality for dependency injection.
    /// </summary>
    internal interface IServiceResolver
    {
        /// <summary>
        /// Resolves an optional service of the specified type.
        /// Returns null if the service is not registered.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <param name="scopeKey">The unique identifier for the scope.</param>
        /// <returns>The service instance, or null if not registered.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="scopeKey"/> is null or empty.</exception>
        T? ResolveService<T>(string scopeKey)
            where T : class;

        /// <summary>
        /// Resolves a required service of the specified type.
        /// Throws an exception if the service is not registered.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <param name="scopeKey">The unique identifier for the scope.</param>
        /// <returns>The service instance.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="scopeKey"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when service provider is not initialized or service is not registered.</exception>
        T ResolveRequiredService<T>(string scopeKey)
            where T : class;
    }
}
