using System;
using Microsoft.Extensions.DependencyInjection;
using Iamfreid.NUnit.DI.Enums;

namespace Iamfreid.NUnit.DI.Core
{
    /// <summary>
    /// Manages service providers and service collections for different scope levels.
    /// </summary>
    internal interface IServiceProviderManager
    {
        /// <summary>
        /// Gets or creates a service collection for the specified scope key.
        /// </summary>
        /// <param name="scopeKey">The unique identifier for the scope.</param>
        /// <returns>The service collection for the specified scope.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="scopeKey"/> is null or empty.</exception>
        IServiceCollection GetServiceCollection(string scopeKey);

        /// <summary>
        /// Initializes a service provider for the specified scope.
        /// </summary>
        /// <param name="scopeKey">The unique identifier for the scope.</param>
        /// <param name="scopeLevel">The scope level (Test, Fixture, or Global).</param>
        /// <param name="autoDispose">Whether to automatically dispose resources based on scope level.</param>
        /// <returns>The initialized service provider.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="scopeKey"/> is null or empty.</exception>
        IServiceProvider Initialize(string scopeKey, ScopeLevel scopeLevel, bool autoDispose);

        /// <summary>
        /// Gets the service provider for the specified scope.
        /// </summary>
        /// <param name="scopeKey">The unique identifier for the scope.</param>
        /// <returns>The service provider, or null if not initialized.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="scopeKey"/> is null or empty.</exception>
        IServiceProvider? GetServiceProvider(string scopeKey);

        /// <summary>
        /// Gets the root service provider (fixture level) for the specified scope.
        /// </summary>
        /// <param name="scopeKey">The unique identifier for the scope.</param>
        /// <returns>The root service provider, or null if not initialized.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="scopeKey"/> is null or empty.</exception>
        IServiceProvider? GetRootServiceProvider(string scopeKey);

        /// <summary>
        /// Creates a test scope for the specified scope key.
        /// </summary>
        /// <param name="scopeKey">The unique identifier for the scope.</param>
        /// <param name="autoDispose">Whether to automatically dispose the scope.</param>
        /// <returns>The service provider for the test scope.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="scopeKey"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when root service provider is not initialized.</exception>
        IServiceProvider CreateTestScope(string scopeKey, bool autoDispose);

        /// <summary>
        /// Disposes the test scope for the specified scope key.
        /// </summary>
        /// <param name="scopeKey">The unique identifier for the scope.</param>
        void DisposeTestScope(string scopeKey);

        /// <summary>
        /// Disposes all resources for the specified scope.
        /// </summary>
        /// <param name="scopeKey">The unique identifier for the scope.</param>
        void Dispose(string scopeKey);

        /// <summary>
        /// Gets the scope level for the specified scope key.
        /// </summary>
        /// <param name="scopeKey">The unique identifier for the scope.</param>
        /// <returns>The scope level, or null if not set.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="scopeKey"/> is null or empty.</exception>
        ScopeLevel? GetScopeLevel(string scopeKey);
    }
}
