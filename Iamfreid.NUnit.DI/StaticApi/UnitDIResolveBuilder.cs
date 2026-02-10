using System;

namespace Iamfreid.NUnit.DI.StaticApi
{
    /// <summary>
    /// Builder for selecting scope level for service resolution.
    /// Provides fluent API for resolving services from different scope levels.
    /// </summary>
    public class UnitDIResolveBuilder
    {
        /// <summary>
        /// Resolves services from the global scope.
        /// </summary>
        /// <returns>The resolve builder for the global scope.</returns>
        public UnitDIResolveBuilderForScope Global()
        {
            var scopeKey = TestScopeKeys.Global();
            return new UnitDIResolveBuilderForScope(scopeKey);
        }

        /// <summary>
        /// Resolves services from a specific fixture scope.
        /// Requires explicit specification of the fixture type.
        /// </summary>
        /// <typeparam name="TFixture">The test fixture type.</typeparam>
        /// <returns>The resolve builder for the fixture scope.</returns>
        public UnitDIResolveBuilderForScope Fixture<TFixture>()
            where TFixture : class
        {
            var scopeKey = TestScopeKeys.ForFixture<TFixture>();
            return new UnitDIResolveBuilderForScope(scopeKey);
        }

        /// <summary>
        /// Resolves services from a specific test scope.
        /// Requires explicit specification of the fixture type.
        /// </summary>
        /// <typeparam name="TFixture">The test fixture type.</typeparam>
        /// <returns>The resolve builder for the test scope.</returns>
        public UnitDIResolveBuilderForScope Test<TFixture>()
            where TFixture : class
        {
            var scopeKey = TestScopeKeys.ForFixture<TFixture>();
            return new UnitDIResolveBuilderForScope(scopeKey);
        }
    }

    /// <summary>
    /// Builder for resolving services from a specific scope.
    /// </summary>
    public class UnitDIResolveBuilderForScope
    {
        private readonly string _scopeKey;

        internal UnitDIResolveBuilderForScope(string scopeKey)
        {
            _scopeKey = scopeKey ?? throw new ArgumentNullException(nameof(scopeKey));
        }

        /// <summary>
        /// Resolves a required service of the specified type.
        /// Throws an exception if the service is not registered.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <returns>The service instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the service is not registered.</exception>
        public T Required<T>()
            where T : class
        {
            return UnitDI.ResolveRequiredService<T>(_scopeKey);
        }

        /// <summary>
        /// Resolves an optional service of the specified type.
        /// Returns null if the service is not registered.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <returns>The service instance, or null if not registered.</returns>
        public T? Optional<T>()
            where T : class
        {
            return UnitDI.ResolveService<T>(_scopeKey);
        }
    }
}
