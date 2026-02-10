using System;
using Microsoft.Extensions.DependencyInjection;
using Iamfreid.NUnit.DI.Enums;

namespace Iamfreid.NUnit.DI.StaticApi
{
    /// <summary>
    /// Extended builder for Global scope that supports both Singleton and Transient lifetimes.
    /// Inherits from <see cref="UnitDIBuilder"/> and adds Transient registration methods.
    /// </summary>
    public sealed class UnitDIGlobalBuilder : UnitDIBuilder
    {
        internal UnitDIGlobalBuilder(string scopeKey, IServiceCollection services)
            : base(scopeKey, services, ScopeLevel.Global)
        {
        }

        /// <summary>
        /// Registers a service as transient.
        /// Available only for Global scope.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <returns>The builder for method chaining.</returns>
        public UnitDIGlobalBuilder AddTransient<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            Services.AddTransient<TService, TImplementation>();
            return this;
        }

        /// <summary>
        /// Registers a service as transient (service and implementation are the same type).
        /// Available only for Global scope.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>The builder for method chaining.</returns>
        public UnitDIGlobalBuilder AddTransient<TService>()
            where TService : class
        {
            Services.AddTransient<TService>();
            return this;
        }

        /// <summary>
        /// Registers a service factory as transient.
        /// Available only for Global scope.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="factory">The factory function.</param>
        /// <returns>The builder for method chaining.</returns>
        public UnitDIGlobalBuilder AddTransientFactory<TService>(Func<IServiceProvider, TService> factory)
            where TService : class
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }
            Services.AddTransient(factory);
            return this;
        }

        /// <summary>
        /// Registers a service as a singleton.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <returns>The builder for method chaining.</returns>
        public new UnitDIGlobalBuilder AddSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            base.AddSingleton<TService, TImplementation>();
            return this;
        }

        /// <summary>
        /// Registers a service as a singleton (service and implementation are the same type).
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>The builder for method chaining.</returns>
        public new UnitDIGlobalBuilder AddSingleton<TService>()
            where TService : class
        {
            base.AddSingleton<TService>();
            return this;
        }

        /// <summary>
        /// Registers a service instance as a singleton.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="instance">The service instance.</param>
        /// <returns>The builder for method chaining.</returns>
        public new UnitDIGlobalBuilder AddInstance<TService>(TService instance)
            where TService : class
        {
            base.AddInstance(instance);
            return this;
        }

        /// <summary>
        /// Registers a service factory as a singleton.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="factory">The factory function.</param>
        /// <returns>The builder for method chaining.</returns>
        public new UnitDIGlobalBuilder AddFactory<TService>(Func<IServiceProvider, TService> factory)
            where TService : class
        {
            base.AddFactory(factory);
            return this;
        }

        /// <summary>
        /// Allows direct access to the underlying service collection for advanced scenarios.
        /// </summary>
        /// <param name="configure">Action to configure the service collection.</param>
        /// <returns>The builder for method chaining.</returns>
        public new UnitDIGlobalBuilder Configure(Action<IServiceCollection> configure)
        {
            base.Configure(configure);
            return this;
        }
    }
}
