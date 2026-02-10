using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Iamfreid.NUnit.DI.Enums;

namespace Iamfreid.NUnit.DI.StaticApi
{
    /// <summary>
    /// Static API for dependency injection in NUnit tests.
    /// Provides methods to register and resolve services using a static API.
    /// </summary>
    public static class UnitDI
    {
        private static readonly ConcurrentDictionary<string, IServiceProvider> _serviceProviders = new();
        private static readonly ConcurrentDictionary<string, IServiceCollection> _serviceCollections = new();
        private static readonly ConcurrentDictionary<string, IServiceScope> _testScopes = new();
        private static readonly ConcurrentDictionary<string, ScopeLevel> _scopeLevels = new();
        private static readonly ILogger _logger = UnitDILogger.GetLogger(nameof(UnitDI));

        /// <summary>
        /// Sets the logger factory for UnitDI logging.
        /// </summary>
        /// <param name="loggerFactory">The logger factory to use. Pass null to disable logging.</param>
        public static void SetLoggerFactory(ILoggerFactory? loggerFactory)
        {
            UnitDILogger.SetLoggerFactory(loggerFactory);
        }

        /// <summary>
        /// Gets the register builder for configuring services at different scope levels.
        /// </summary>
        public static UnitDIRegisterBuilder Register => new UnitDIRegisterBuilder();

        /// <summary>
        /// Gets the resolve builder for resolving services from different scope levels.
        /// </summary>
        public static UnitDIResolveBuilder Resolve => new UnitDIResolveBuilder();

        /// <summary>
        /// Gets or creates a service collection for the specified scope key.
        /// </summary>
        /// <param name="scopeKey">The key identifying the scope (e.g., fixture name).</param>
        /// <returns>The service collection for the specified scope.</returns>
        internal static IServiceCollection GetServiceCollection(string scopeKey)
        {
            if (string.IsNullOrWhiteSpace(scopeKey))
            {
                throw new ArgumentException("Scope key cannot be null or empty.", nameof(scopeKey));
            }

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

        /// <summary>
        /// Registers a service in the specified scope.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="scopeKey">The scope key.</param>
        /// <param name="lifetime">The service lifetime.</param>
        /// <returns>The service collection for chaining.</returns>
        internal static IServiceCollection RegisterService<TService, TImplementation>(
            string scopeKey,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TService : class
            where TImplementation : class, TService
        {
            if (string.IsNullOrWhiteSpace(scopeKey))
            {
                throw new ArgumentException("Scope key cannot be null or empty.", nameof(scopeKey));
            }

            var services = GetServiceCollection(scopeKey);
            AddServiceWithLifetime(services, lifetime, () => services.AddSingleton<TService, TImplementation>(), 
                () => services.AddScoped<TService, TImplementation>(), 
                () => services.AddTransient<TService, TImplementation>());

            return services;
        }

        /// <summary>
        /// Registers a service instance in the specified scope.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="scopeKey">The scope key.</param>
        /// <param name="instance">The service instance.</param>
        /// <returns>The service collection for chaining.</returns>
        internal static IServiceCollection RegisterInstance<TService>(
            string scopeKey,
            TService instance)
            where TService : class
        {
            if (string.IsNullOrWhiteSpace(scopeKey))
            {
                throw new ArgumentException("Scope key cannot be null or empty.", nameof(scopeKey));
            }
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var services = GetServiceCollection(scopeKey);
            services.AddSingleton(instance);
            return services;
        }

        /// <summary>
        /// Registers a service factory in the specified scope.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="scopeKey">The scope key.</param>
        /// <param name="factory">The factory function.</param>
        /// <param name="lifetime">The service lifetime.</param>
        /// <returns>The service collection for chaining.</returns>
        internal static IServiceCollection RegisterFactory<TService>(
            string scopeKey,
            Func<IServiceProvider, TService> factory,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TService : class
        {
            if (string.IsNullOrWhiteSpace(scopeKey))
            {
                throw new ArgumentException("Scope key cannot be null or empty.", nameof(scopeKey));
            }
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var services = GetServiceCollection(scopeKey);
            AddServiceWithLifetime(services, lifetime, () => services.AddSingleton(factory), 
                () => services.AddScoped(factory), 
                () => services.AddTransient(factory));

            return services;
        }

        /// <summary>
        /// Initializes the service provider for the specified scope.
        /// </summary>
        /// <param name="scopeKey">The scope key.</param>
        /// <param name="scopeLevel">The scope level.</param>
        /// <param name="autoDispose">If true, resources will be automatically disposed based on scope level.</param>
        /// <returns>The service provider.</returns>
        internal static IServiceProvider Initialize(string scopeKey, ScopeLevel scopeLevel = ScopeLevel.Fixture, bool autoDispose = true)
        {
            if (string.IsNullOrWhiteSpace(scopeKey))
            {
                throw new ArgumentException("Scope key cannot be null or empty.", nameof(scopeKey));
            }

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

        /// <summary>
        /// Gets the service provider for the specified scope.
        /// </summary>
        /// <param name="scopeKey">The scope key.</param>
        /// <returns>The service provider, or null if not initialized.</returns>
        internal static IServiceProvider? GetServiceProvider(string scopeKey)
        {
            if (string.IsNullOrWhiteSpace(scopeKey))
            {
                throw new ArgumentException("Scope key cannot be null or empty.", nameof(scopeKey));
            }

            if (_testScopes.TryGetValue(scopeKey, out var testScope))
            {
                return testScope.ServiceProvider;
            }

            _serviceProviders.TryGetValue(scopeKey, out var provider);
            return provider;
        }

        /// <summary>
        /// Gets the root service provider (fixture/assembly level) for the specified scope.
        /// </summary>
        /// <param name="scopeKey">The scope key.</param>
        /// <returns>The root service provider, or null if not initialized.</returns>
        internal static IServiceProvider? GetRootServiceProvider(string scopeKey)
        {
            if (string.IsNullOrWhiteSpace(scopeKey))
            {
                throw new ArgumentException("Scope key cannot be null or empty.", nameof(scopeKey));
            }

            _serviceProviders.TryGetValue(scopeKey, out var provider);
            return provider;
        }

        internal static T? ResolveService<T>(string scopeKey)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(scopeKey))
            {
                throw new ArgumentException("Scope key cannot be null or empty.", nameof(scopeKey));
            }

            _logger.LogDebug("Resolving service {ServiceType} from scope {ScopeKey}", typeof(T).Name, scopeKey);

            var provider = GetServiceProvider(scopeKey);
            var service = provider?.GetService<T>();

            if (service == null)
            {
                _logger.LogWarning("Service {ServiceType} not found in scope {ScopeKey}", typeof(T).Name, scopeKey);
            }
            else
            {
                _logger.LogDebug("Successfully resolved service {ServiceType} from scope {ScopeKey}", typeof(T).Name, scopeKey);
            }

            return service;
        }

        internal static T ResolveRequiredService<T>(string scopeKey)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(scopeKey))
            {
                throw new ArgumentException("Scope key cannot be null or empty.", nameof(scopeKey));
            }

            _logger.LogDebug("Resolving required service {ServiceType} from scope {ScopeKey}", typeof(T).Name, scopeKey);

            var provider = GetServiceProvider(scopeKey);
            if (provider == null)
            {
                _logger.LogError("Service provider not initialized for scope key: {ScopeKey}", scopeKey);
                throw new InvalidOperationException(
                    $"ServiceProvider is not initialized for scope '{scopeKey}'. Call Initialize() first.");
            }

            try
            {
                var service = provider.GetService<T>();
                if (service == null)
                {
                    _logger.LogError("Service {ServiceType} is not registered in scope {ScopeKey}", typeof(T).Name, scopeKey);
                    throw new InvalidOperationException(
                        $"Service of type {typeof(T).Name} is not registered in scope '{scopeKey}'.");
                }

                _logger.LogDebug("Successfully resolved required service {ServiceType} from scope {ScopeKey}", typeof(T).Name, scopeKey);
                return service;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve required service {ServiceType} from scope {ScopeKey}", typeof(T).Name, scopeKey);
                throw;
            }
        }

        internal static IServiceProvider CreateTestScope(string scopeKey, bool autoDispose = true)
        {
            if (string.IsNullOrWhiteSpace(scopeKey))
            {
                throw new ArgumentException("Scope key cannot be null or empty.", nameof(scopeKey));
            }

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

        internal static void DisposeTestScope(string scopeKey)
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

        internal static void Dispose(string scopeKey)
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

        /// <summary>
        /// Helper method to add a service to the collection with the specified lifetime.
        /// Scoped lifetime is not supported in this library.
        /// </summary>
        private static void AddServiceWithLifetime(IServiceCollection services, ServiceLifetime lifetime,
            Action addSingleton, Action addScoped, Action addTransient)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    addSingleton();
                    break;
                case ServiceLifetime.Scoped:
                    throw new NotSupportedException(
                        "Scoped lifetime is not supported. Use Singleton or Transient instead.");
                case ServiceLifetime.Transient:
                    addTransient();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "Unknown service lifetime.");
            }
        }
    }
}

