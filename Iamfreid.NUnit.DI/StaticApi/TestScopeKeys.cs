using System;

namespace Iamfreid.NUnit.DI.StaticApi
{
    /// <summary>
    /// Helper class for generating scope keys for different NUnit test levels.
    /// </summary>
    public static class TestScopeKeys
    {
        /// <summary>
        /// Generates a scope key for a test fixture.
        /// </summary>
        /// <param name="fixtureType">The test fixture type. Must not be null.</param>
        /// <returns>
        /// A scope key in the format: <c>Fixture:&lt;FullTypeName&gt;</c>
        /// </returns>
        public static string ForFixture(Type fixtureType)
        {
            if (fixtureType == null)
            {
                throw new ArgumentNullException(nameof(fixtureType));
            }

            return $"Fixture:{fixtureType.FullName}";
        }

        /// <summary>
        /// Generates a scope key for a test fixture using a generic type parameter.
        /// </summary>
        /// <typeparam name="T">The test fixture type.</typeparam>
        /// <returns>
        /// A scope key in the format: <c>Fixture:&lt;FullTypeName&gt;</c>
        /// </returns>
        public static string ForFixture<T>()
        {
            return ForFixture(typeof(T));
        }

        /// <summary>
        /// Generates a scope key for a specific test method within a test fixture.
        /// </summary>
        /// <param name="fixtureType">The test fixture type. Must not be null.</param>
        /// <param name="testMethodName">The test method name. Must not be null or empty.</param>
        /// <returns>
        /// A scope key in the format: <c>Test:&lt;FullTypeName&gt;:&lt;MethodName&gt;</c>
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fixtureType"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="testMethodName"/> is null, empty, or whitespace.
        /// </exception>
        public static string ForTest(Type fixtureType, string testMethodName)
        {
            if (fixtureType == null)
            {
                throw new ArgumentNullException(nameof(fixtureType));
            }

            if (string.IsNullOrWhiteSpace(testMethodName))
            {
                throw new ArgumentException(
                    "Test method name cannot be null or empty.",
                    nameof(testMethodName));
            }

            return $"Test:{fixtureType.FullName}:{testMethodName}";
        }

        /// <summary>
        /// Generates a scope key for the global scope.
        /// </summary>
        /// <returns>
        /// A scope key with the value: <c>Global</c>
        /// </returns>
        public static string Global()
        {
            return "Global";
        }
    }
}




