using System;

namespace Iamfreid.NUnit.DI.Core
{
    /// <summary>
    /// Resolves scope keys from the call stack or test context.
    /// </summary>
    internal interface IScopeKeyResolver
    {
        /// <summary>
        /// Resolves the fixture type from the call stack by finding the first class with [TestFixture] attribute.
        /// </summary>
        /// <returns>The fixture type, or null if not found.</returns>
        Type? ResolveFixtureTypeFromStack();

        /// <summary>
        /// Resolves the test method name from TestContext.
        /// </summary>
        /// <returns>The test method name, or null if not found.</returns>
        string? ResolveTestMethodName();
    }
}
