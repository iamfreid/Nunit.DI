using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Iamfreid.NUnit.DI.Core
{
    /// <summary>
    /// Provides service registration functionality for dependency injection.
    /// </summary>
    internal sealed class ServiceRegistrar : IServiceRegistrar
    {
        private readonly IServiceProviderManager _serviceProviderManager;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistrar"/> class.
        /// </summary>
        /// <param name="serviceProviderManager">The service provider manager for accessing service collections.</param>
        /// <param name="logger">The logger instance for logging operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public ServiceRegistrar(IServiceProviderManager serviceProviderManager, ILogger logger)
        {
            _serviceProviderManager = serviceProviderManager ?? throw new ArgumentNullException(nameof(serviceProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public IServiceCollection RegisterService<TService, TImplementation>(
            string scopeKey,
            ServiceLifetime lifetime)
            where TService : class
            where TImplementation : class, TService
        {
            ValidateScopeKey(scopeKey);

            var services = _serviceProviderManager.GetServiceCollection(scopeKey);
            AddServiceWithLifetime(services, lifetime,
                () => services.AddSingleton<TService, TImplementation>(),
                () => throw new NotSupportedException("Scoped lifetime is not supported. Use Singleton or Transient instead."),
                () => services.AddTransient<TService, TImplementation>());

            _logger.LogDebug(
                "Registered service {ServiceType} -> {ImplementationType} with lifetime {Lifetime} for scope {ScopeKey}",
                typeof(TService).Name, typeof(TImplementation).Name, lifetime, scopeKey);

            return services;
        }

        /// <inheritdoc />
        public IServiceCollection RegisterInstance<TService>(string scopeKey, TService instance)
            where TService : class
        {
            ValidateScopeKey(scopeKey);

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var services = _serviceProviderManager.GetServiceCollection(scopeKey);
            services.AddSingleton(instance);

            _logger.LogDebug(
                "Registered service instance {ServiceType} for scope {ScopeKey}",
                typeof(TService).Name, scopeKey);

            return services;
        }

        /// <inheritdoc />
        public IServiceCollection RegisterFactory<TService>(
            string scopeKey,
            Func<IServiceProvider, TService> factory,
            ServiceLifetime lifetime)
            where TService : class
        {
            ValidateScopeKey(scopeKey);

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var services = _serviceProviderManager.GetServiceCollection(scopeKey);
            AddServiceWithLifetime(services, lifetime,
                () => services.AddSingleton(factory),
                () => throw new NotSupportedException("Scoped lifetime is not supported. Use Singleton or Transient instead."),
                () => services.AddTransient(factory));

            _logger.LogDebug(
                "Registered service factory {ServiceType} with lifetime {Lifetime} for scope {ScopeKey}",
                typeof(TService).Name, lifetime, scopeKey);

            return services;
        }

        /// <summary>
        /// Helper method to add a service to the collection with the specified lifetime.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="lifetime">The service lifetime.</param>
        /// <param name="addSingleton">Action to add as singleton.</param>
        /// <param name="addScoped">Action to add as scoped (not supported).</param>
        /// <param name="addTransient">Action to add as transient.</param>
        /// <exception cref="NotSupportedException">Thrown when Scoped lifetime is used.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when lifetime is unknown.</exception>
        private static void AddServiceWithLifetime(
            IServiceCollection services,
            ServiceLifetime lifetime,
            Action addSingleton,
            Action addScoped,
            Action addTransient)
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
