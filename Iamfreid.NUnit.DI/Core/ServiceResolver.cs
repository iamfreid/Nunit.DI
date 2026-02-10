using System;
using Microsoft.Extensions.Logging;

namespace Iamfreid.NUnit.DI.Core
{
    /// <summary>
    /// Provides service resolution functionality for dependency injection.
    /// Implements the ServiceResolver pattern following the Single Responsibility Principle.
    /// </summary>
    internal sealed class ServiceResolver : IServiceResolver
    {
        private readonly IServiceProviderManager _serviceProviderManager;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceResolver"/> class.
        /// </summary>
        /// <param name="serviceProviderManager">The service provider manager for accessing service providers.</param>
        /// <param name="logger">The logger instance for logging operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public ServiceResolver(IServiceProviderManager serviceProviderManager, ILogger logger)
        {
            _serviceProviderManager = serviceProviderManager ?? throw new ArgumentNullException(nameof(serviceProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public T? ResolveService<T>(string scopeKey)
            where T : class
        {
            ValidateScopeKey(scopeKey);

            _logger.LogDebug("Resolving service {ServiceType} from scope {ScopeKey}", typeof(T).Name, scopeKey);

            var provider = _serviceProviderManager.GetServiceProvider(scopeKey);
            var service = provider?.GetService(typeof(T)) as T;

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

        /// <inheritdoc />
        public T ResolveRequiredService<T>(string scopeKey)
            where T : class
        {
            ValidateScopeKey(scopeKey);

            _logger.LogDebug("Resolving required service {ServiceType} from scope {ScopeKey}", typeof(T).Name, scopeKey);

            var provider = _serviceProviderManager.GetServiceProvider(scopeKey);
            if (provider == null)
            {
                var message = $"ServiceProvider is not initialized for scope '{scopeKey}'. Call Initialize() first.";
                _logger.LogError("Service provider not initialized for scope key: {ScopeKey}", scopeKey);
                throw new InvalidOperationException(message);
            }

            try
            {
                var service = provider.GetService(typeof(T)) as T;
                if (service == null)
                {
                    var message = $"Service of type {typeof(T).Name} is not registered in scope '{scopeKey}'.";
                    _logger.LogError("Service {ServiceType} is not registered in scope {ScopeKey}", typeof(T).Name, scopeKey);
                    throw new InvalidOperationException(message);
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
