using System;
using Microsoft.Extensions.DependencyInjection;
using Iamfreid.NUnit.DI.Enums;

namespace Iamfreid.NUnit.DI.StaticApi
{
    /// <summary>
    /// Builder for configuring services in a fluent, readable way.
    /// Provides a more intuitive API for service registration.
    /// </summary>
    public class UnitDIBuilder
    {
        private readonly string _scopeKey;
        private readonly IServiceCollection _services;
        private readonly ScopeLevel? _scopeLevel;
        private bool _isBuilt;

        internal UnitDIBuilder(string scopeKey, IServiceCollection services)
        {
            _scopeKey = scopeKey ?? throw new ArgumentNullException(nameof(scopeKey));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _scopeLevel = null;
        }

        internal UnitDIBuilder(string scopeKey, IServiceCollection services, ScopeLevel scopeLevel)
        {
            _scopeKey = scopeKey ?? throw new ArgumentNullException(nameof(scopeKey));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _scopeLevel = scopeLevel;
        }

        /// <summary>
        /// Registers a service as a singleton.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <returns>The builder for method chaining.</returns>
        public UnitDIBuilder AddSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            _services.AddSingleton<TService, TImplementation>();
            return this;
        }

        /// <summary>
        /// Registers a service as a singleton (service and implementation are the same type).
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>The builder for method chaining.</returns>
        public UnitDIBuilder AddSingleton<TService>()
            where TService : class
        {
            _services.AddSingleton<TService>();
            return this;
        }

        /// <summary>
        /// Registers a service instance as a singleton.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="instance">The service instance.</param>
        /// <returns>The builder for method chaining.</returns>
        public UnitDIBuilder AddInstance<TService>(TService instance)
            where TService : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            _services.AddSingleton(instance);
            return this;
        }

        /// <summary>
        /// Registers a service factory as a singleton.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="factory">The factory function.</param>
        /// <returns>The builder for method chaining.</returns>
        public UnitDIBuilder AddFactory<TService>(Func<IServiceProvider, TService> factory)
            where TService : class
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }
            _services.AddSingleton(factory);
            return this;
        }


        /// <summary>
        /// Allows direct access to the underlying service collection for advanced scenarios.
        /// </summary>
        /// <param name="configure">Action to configure the service collection.</param>
        /// <returns>The builder for method chaining.</returns>
        public UnitDIBuilder Configure(Action<IServiceCollection> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            configure(_services);
            return this;
        }

        /// <summary>
        /// Gets the underlying service collection for advanced scenarios.
        /// </summary>
        public IServiceCollection Services => _services;

        /// <summary>
        /// Gets the scope key for this builder.
        /// </summary>
        public string ScopeKey => _scopeKey;

        /// <summary>
        /// Builds and initializes the service provider.
        /// This method must be called to finalize the configuration and make services available.
        /// </summary>
        /// <param name="autoDispose">If true, resources will be automatically disposed based on scope level.</param>
        /// <returns>The service provider.</returns>
        public IServiceProvider Build(bool autoDispose = true)
        {
            if (_isBuilt)
            {
                return UnitDI.GetServiceProvider(_scopeKey) 
                    ?? throw new InvalidOperationException($"Service provider for scope '{_scopeKey}' was disposed.");
            }

            if (_scopeLevel == null)
            {
                throw new InvalidOperationException(
                    "Cannot build without scope level. Use UnitDI.Register.Global(), UnitDI.Register.Fixture<TFixture>(), or UnitDI.Register.Test<TFixture>() to create a builder with scope level.");
            }

            _isBuilt = true;
            return UnitDI.Initialize(_scopeKey, _scopeLevel.Value, autoDispose);
        }
    }
}

