using Microsoft.Extensions.DependencyInjection;

namespace Iamfreid.NUnit.DI.Extensions
{
    /// <summary>
    /// Extension methods for IServiceCollection to simplify service registration in tests.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a service as a singleton.
        /// </summary>
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services)
            where TService : class
        {
            return services.AddSingleton<TService, TService>();
        }

        /// <summary>
        /// Registers a service as a singleton with an implementation type.
        /// </summary>
        public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            return services.AddSingleton<TService, TImplementation>();
        }

        /// <summary>
        /// Registers a service as a singleton with a factory.
        /// </summary>
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, TService instance)
            where TService : class
        {
            return services.AddSingleton<TService>(_ => instance);
        }

        /// <summary>
        /// Registers a service as transient.
        /// </summary>
        public static IServiceCollection AddTransient<TService>(this IServiceCollection services)
            where TService : class
        {
            return services.AddTransient<TService, TService>();
        }

        /// <summary>
        /// Registers a service as transient with an implementation type.
        /// </summary>
        public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            return services.AddTransient<TService, TImplementation>();
        }

        /// <summary>
        /// Registers a service as scoped.
        /// </summary>
        public static IServiceCollection AddScoped<TService>(this IServiceCollection services)
            where TService : class
        {
            return services.AddScoped<TService, TService>();
        }

        /// <summary>
        /// Registers a service as scoped with an implementation type.
        /// </summary>
        public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            return services.AddScoped<TService, TImplementation>();
        }
    }
}

