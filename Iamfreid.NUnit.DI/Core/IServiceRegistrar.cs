using System;
using Microsoft.Extensions.DependencyInjection;

namespace Iamfreid.NUnit.DI.Core
{
    /// <summary>
    /// Provides service registration functionality for dependency injection.
    /// </summary>
    internal interface IServiceRegistrar
    {
        /// <summary>
        /// Registers a service with the specified lifetime.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="scopeKey">The unique identifier for the scope.</param>
        /// <param name="lifetime">The service lifetime (Singleton or Transient).</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="scopeKey"/> is null or empty.</exception>
        IServiceCollection RegisterService<TService, TImplementation>(
            string scopeKey,
            ServiceLifetime lifetime)
            where TService : class
            where TImplementation : class, TService;

        /// <summary>
        /// Registers a service instance as a singleton.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="scopeKey">The unique identifier for the scope.</param>
        /// <param name="instance">The service instance to register.</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="scopeKey"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        IServiceCollection RegisterInstance<TService>(string scopeKey, TService instance)
            where TService : class;

        /// <summary>
        /// Registers a service factory with the specified lifetime.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="scopeKey">The unique identifier for the scope.</param>
        /// <param name="factory">The factory function to create the service.</param>
        /// <param name="lifetime">The service lifetime (Singleton or Transient).</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="scopeKey"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> is null.</exception>
        IServiceCollection RegisterFactory<TService>(
            string scopeKey,
            Func<IServiceProvider, TService> factory,
            ServiceLifetime lifetime)
            where TService : class;
    }
}
