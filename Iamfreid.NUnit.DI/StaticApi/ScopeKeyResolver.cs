using System;
using System.Diagnostics;
using System.Reflection;
using Iamfreid.NUnit.DI.Core;
using NUnit.Framework;

namespace Iamfreid.NUnit.DI.StaticApi
{
    /// <summary>
    /// Resolves scope keys automatically from the call stack or test context.
    /// Implements the ScopeKeyResolver pattern following the Single Responsibility Principle.
    /// </summary>
    internal sealed class ScopeKeyResolver : IScopeKeyResolver
    {
        /// <summary>
        /// The default instance of the scope key resolver.
        /// </summary>
        internal static readonly IScopeKeyResolver Default = new ScopeKeyResolver();

        /// <summary>
        /// Prevents external instantiation. Use <see cref="Default"/> instead.
        /// </summary>
        private ScopeKeyResolver()
        {
        }

        /// <inheritdoc />
        public Type? ResolveFixtureTypeFromStack()
        {
            var fixtureType = FindFixtureTypeInStackTrace();
            if (fixtureType != null)
            {
                return fixtureType;
            }

            return GetFixtureTypeFromTestContext();
        }

        /// <inheritdoc />
        public string? ResolveTestMethodName()
        {
            try
            {
                return TestContext.CurrentContext?.Test?.Name;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Finds the fixture type in the current stack trace by searching for classes with [TestFixture] attribute.
        /// </summary>
        /// <returns>The fixture type, or null if not found.</returns>
        private static Type? FindFixtureTypeInStackTrace()
        {
            var stackTrace = new StackTrace(skipFrames: 2);

            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                if (frame?.GetMethod()?.DeclaringType is not Type declaringType)
                {
                    continue;
                }

                if (declaringType.GetCustomAttribute<TestFixtureAttribute>() != null)
                {
                    return declaringType;
                }

                var baseType = declaringType.BaseType;
                while (baseType != null && baseType != typeof(object))
                {
                    if (baseType.GetCustomAttribute<TestFixtureAttribute>() != null)
                    {
                        return baseType;
                    }
                    baseType = baseType.BaseType;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the fixture type from NUnit's TestContext as a fallback mechanism.
        /// </summary>
        /// <returns>The fixture type, or null if not found or TestContext is unavailable.</returns>
        private static Type? GetFixtureTypeFromTestContext()
        {
            try
            {
                var type = GetTypeFromTestContext();
                if (type != null && type.GetCustomAttribute<TestFixtureAttribute>() != null)
                {
                    return type;
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }

        /// <summary>
        /// Gets the type from TestContext's current test class name.
        /// </summary>
        /// <returns>The type, or null if not found or TestContext is unavailable.</returns>
        private static Type? GetTypeFromTestContext()
        {
            try
            {
                var testType = TestContext.CurrentContext?.Test?.ClassName;
                if (!string.IsNullOrEmpty(testType))
                {
                    return Type.GetType(testType);
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }
    }
}




