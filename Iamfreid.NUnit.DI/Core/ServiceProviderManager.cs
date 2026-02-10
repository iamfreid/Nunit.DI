using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Iamfreid.NUnit.DI.Enums;
using Iamfreid.NUnit.DI.StaticApi;

namespace Iamfreid.NUnit.DI.Core
{
    /// <summary>
    /// Manages service providers and service collections for different scope levels.
    /// </summary>
    internal sealed class ServiceProviderManager : IServiceProviderManager
    {
        private readonly ConcurrentDictionary<string, IServiceProvider> _serviceProviders = new();
        private readonly ConcurrentDictionary<string, IServiceCollection> _serviceCollections = new();
        private readonly ConcurrentDictionary<string, IServiceScope> _testScopes = new();
        private readonly ConcurrentDictionary<string, ScopeLevel> _scopeLevels = new();
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderManager"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
        public ServiceProviderManager(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public IServiceCollection GetServiceCollection(string scopeKey)
        {
            ValidateScopeKey(scopeKey);

            var isNew = !_serviceCollections.ContainsKey(scopeKey);
            var services = _serviceCollections.GetOrAdd(scopeKey, _ =>
            {
                _logger.LogDebug("Creating new service collection for scope key: {ScopeKey}", scopeKey);
                return new ServiceCollection();
            });

            if (!isNew)
            {
                _logger.LogDebug("Retrieved existing service collection for scope key: {ScopeKey}", scopeKey);
            }

            return services;
        }

        /// <inheritdoc />
        public IServiceProvider Initialize(string scopeKey, ScopeLevel scopeLevel, bool autoDispose)
        {
            ValidateScopeKey(scopeKey);

            _logger.LogInformation(
                "Initializing service provider for scope {ScopeKey} with level {ScopeLevel}, autoDispose: {AutoDispose}",
                scopeKey, scopeLevel, autoDispose);

            _scopeLevels[scopeKey] = scopeLevel;

            var services = GetServiceCollection(scopeKey);
            var serviceCount = services.Count;
            var serviceProvider = services.BuildServiceProvider();

            _serviceProviders[scopeKey] = serviceProvider;

            _logger.LogDebug(
                "Built service provider for scope {ScopeKey} with {ServiceCount} registered services",
                scopeKey, serviceCount);

            if (autoDispose)
            {
                AutoDisposeManager.RegisterScope(scopeKey, scopeLevel);

                if (scopeLevel == ScopeLevel.Fixture || scopeLevel == ScopeLevel.Global)
                {
                    AutoDisposeManager.SetCurrentFixtureKey(scopeKey);
                }
            }

            if (scopeLevel == ScopeLevel.Test)
            {
                CreateTestScope(scopeKey, autoDispose);
            }

            _logger.LogInformation(
                "Service provider initialized for scope {ScopeKey} ({ScopeLevel})",
                scopeKey, scopeLevel);

            return serviceProvider;
        }

        /// <inheritdoc />
        public IServiceProvider? GetServiceProvider(string scopeKey)
        {
            ValidateScopeKey(scopeKey);

            if (_testScopes.TryGetValue(scopeKey, out var testScope))
            {
                return testScope.ServiceProvider;
            }

            _serviceProviders.TryGetValue(scopeKey, out var provider);
            return provider;
        }

        /// <inheritdoc />
        public IServiceProvider? GetRootServiceProvider(string scopeKey)
        {
            ValidateScopeKey(scopeKey);

            _serviceProviders.TryGetValue(scopeKey, out var provider);
            return provider;
        }

        /// <inheritdoc />
        public IServiceProvider CreateTestScope(string scopeKey, bool autoDispose = true)
        {
            ValidateScopeKey(scopeKey);

            var rootProvider = GetRootServiceProvider(scopeKey);
            if (rootProvider == null)
            {
                throw new InvalidOperationException(
                    $"Root ServiceProvider is not initialized for scope '{scopeKey}'. Call Initialize() first.");
            }

            DisposeTestScope(scopeKey);
            var testScope = rootProvider.CreateScope();
            _testScopes[scopeKey] = testScope;

            if (autoDispose)
            {
                AutoDisposeManager.SetCurrentTestKey(scopeKey);
            }

            return testScope.ServiceProvider;
        }

        /// <inheritdoc />
        public void DisposeTestScope(string scopeKey)
        {
            if (string.IsNullOrWhiteSpace(scopeKey))
            {
                return;
            }

            if (_testScopes.TryRemove(scopeKey, out var testScope))
            {
                _logger.LogDebug("Disposing test scope for {ScopeKey}", scopeKey);
                testScope.Dispose();
                _logger.LogInformation("Disposed test scope for {ScopeKey}", scopeKey);
            }
            else
            {
                _logger.LogDebug("Test scope for {ScopeKey} was not found or already disposed", scopeKey);
            }
        }

        /// <inheritdoc />
        public void Dispose(string scopeKey)
        {
            if (string.IsNullOrWhiteSpace(scopeKey))
            {
                return;
            }

            _logger.LogInformation("Disposing all resources for scope {ScopeKey}", scopeKey);

            DisposeTestScope(scopeKey);

            if (_serviceProviders.TryRemove(scopeKey, out var provider) && provider is IDisposable disposable)
            {
                _logger.LogDebug("Disposing service provider for {ScopeKey}", scopeKey);
                disposable.Dispose();
            }

            _serviceCollections.TryRemove(scopeKey, out _);
            _scopeLevels.TryRemove(scopeKey, out _);

            _logger.LogInformation("Disposed all resources for scope {ScopeKey}", scopeKey);
        }

        /// <inheritdoc />
        public ScopeLevel? GetScopeLevel(string scopeKey)
        {
            ValidateScopeKey(scopeKey);

            _scopeLevels.TryGetValue(scopeKey, out var level);
            return level;
        }

        /// <summary>
        /// Validates that the scope key is not null or empty.
        /// </summary>
        /// <param name="scopeKey">The scope key to validate.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="scopeKey"/> is null or empty.</exception>
        private static void ValidateScopeKey(string scopeKey)
        {
            if (string.IsNullOrWhiteSpace(scopeKey))
            {
                throw new ArgumentException("Scope key cannot be null or empty.", nameof(scopeKey));
            }
        }
    }
}
