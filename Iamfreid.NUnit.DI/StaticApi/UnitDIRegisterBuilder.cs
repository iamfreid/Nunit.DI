using System;
using Iamfreid.NUnit.DI.Enums;

namespace Iamfreid.NUnit.DI.StaticApi
{
    /// <summary>
    /// Builder for selecting scope level for service registration.
    /// Provides a fluent API for configuring services at different scope levels.
    /// </summary>
    public sealed class UnitDIRegisterBuilder
    {
        /// <summary>
        /// Configures services for the global scope.
        /// </summary>
        /// <returns>
        /// A <see cref="UnitDIGlobalBuilder"/> instance configured for the global scope.
        /// This builder supports both Singleton and Transient lifetimes.
        /// </returns>
        public UnitDIGlobalBuilder Global()
        {
            var scopeKey = TestScopeKeys.Global();
            var services = UnitDI.GetServiceCollection(scopeKey);
            return new UnitDIGlobalBuilder(scopeKey, services);
        }

        /// <summary>
        /// Configures services for a specific fixture scope.
        /// Requires explicit specification of the fixture type through a generic type parameter.
        /// </summary>
        /// <typeparam name="TFixture">The test fixture type. Must be a class.</typeparam>
        /// <returns>
        /// A <see cref="UnitDIBuilder"/> instance configured for the fixture scope.
        /// </returns>
        public UnitDIBuilder Fixture<TFixture>()
            where TFixture : class
        {
            var scopeKey = TestScopeKeys.ForFixture<TFixture>();
            var services = UnitDI.GetServiceCollection(scopeKey);
            return new UnitDIBuilder(scopeKey, services, ScopeLevel.Fixture);
        }

        /// <summary>
        /// Configures services for a specific test scope.
        /// Requires explicit specification of the fixture type through a generic type parameter.
        /// </summary>
        /// <typeparam name="TFixture">The test fixture type. Must be a class.</typeparam>
        /// <returns>
        /// A <see cref="UnitDIBuilder"/> instance configured for the test scope.
        /// </returns>
        public UnitDIBuilder Test<TFixture>()
            where TFixture : class
        {
            var scopeKey = TestScopeKeys.ForFixture<TFixture>();
            var services = UnitDI.GetServiceCollection(scopeKey);
            return new UnitDIBuilder(scopeKey, services, ScopeLevel.Test);
        }
    }
}
