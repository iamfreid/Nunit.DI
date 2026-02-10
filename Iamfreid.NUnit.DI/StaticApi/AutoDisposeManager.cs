using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Iamfreid.NUnit.DI.Enums;

namespace Iamfreid.NUnit.DI.StaticApi
{
    /// <summary>
    /// Manages automatic disposal of DI resources based on NUnit test lifecycle.
    /// </summary>
    internal static class AutoDisposeManager
    {
        private static readonly AsyncLocal<string?> _currentFixtureKey = new();
        private static readonly AsyncLocal<string?> _currentTestKey = new();
        private static readonly ConcurrentDictionary<string, ScopeInfo> _activeScopes = new();
        private static readonly ILogger _logger = UnitDILogger.GetLogger(nameof(AutoDisposeManager));

        private class ScopeInfo
        {
            public string Key { get; set; } = null!;
            public ScopeLevel Level { get; set; }
        }

        /// <summary>
        /// Registers a scope for automatic disposal.
        /// </summary>
        internal static void RegisterScope(string scopeKey, ScopeLevel scopeLevel)
        {
            _logger.LogDebug("Registering scope {ScopeKey} with level {ScopeLevel} for automatic disposal", scopeKey, scopeLevel);
            _activeScopes.TryAdd(scopeKey, new ScopeInfo
            {
                Key = scopeKey,
                Level = scopeLevel
            });
            _logger.LogInformation("Registered scope {ScopeKey} ({ScopeLevel}) for automatic disposal", scopeKey, scopeLevel);
        }

        /// <summary>
        /// Sets the current fixture scope key for the test context.
        /// </summary>
        internal static void SetCurrentFixtureKey(string? scopeKey)
        {
            _currentFixtureKey.Value = scopeKey;
        }

        /// <summary>
        /// Sets the current test scope key for the test context.
        /// </summary>
        internal static void SetCurrentTestKey(string? scopeKey)
        {
            _currentTestKey.Value = scopeKey;
        }

        /// <summary>
        /// Gets the current fixture scope key.
        /// </summary>
        internal static string? GetCurrentFixtureKey()
        {
            return _currentFixtureKey.Value;
        }

        /// <summary>
        /// Gets the current test scope key.
        /// </summary>
        internal static string? GetCurrentTestKey()
        {
            return _currentTestKey.Value;
        }

        /// <summary>
        /// Automatically disposes test scope for the current test.
        /// </summary>
        internal static void AutoDisposeTestScope()
        {
            var testKey = _currentTestKey.Value;
            if (testKey != null)
            {
                _logger.LogDebug("Auto-disposing test scope: {TestKey}", testKey);
                UnitDI.DisposeTestScope(testKey);
                _currentTestKey.Value = null;
                _logger.LogInformation("Auto-disposed test scope: {TestKey}", testKey);
            }
        }

        /// <summary>
        /// Automatically disposes fixture scope for the current fixture.
        /// </summary>
        internal static void AutoDisposeFixtureScope()
        {
            var fixtureKey = _currentFixtureKey.Value;
            if (fixtureKey != null)
            {
                _logger.LogDebug("Auto-disposing fixture scope: {FixtureKey}", fixtureKey);
                UnitDI.Dispose(fixtureKey);
                _activeScopes.TryRemove(fixtureKey, out _);
                _currentFixtureKey.Value = null;
                _logger.LogInformation("Auto-disposed fixture scope: {FixtureKey}", fixtureKey);
            }
        }

        /// <summary>
        /// Automatically disposes all scopes for the current test context.
        /// </summary>
        internal static void AutoDisposeAll()
        {
            _logger.LogDebug("Auto-disposing all scopes for current test context");
            AutoDisposeTestScope();
            AutoDisposeFixtureScope();
            _logger.LogInformation("Auto-disposed all scopes for current test context");
        }

        /// <summary>
        /// Cleans up all registered scopes (for global cleanup).
        /// </summary>
        internal static void CleanupAll()
        {
            var keys = _activeScopes.Keys.ToArray();
            _logger.LogInformation("Cleaning up {Count} registered scopes", keys.Length);
            foreach (var key in keys)
            {
                _logger.LogDebug("Disposing scope: {ScopeKey}", key);
                UnitDI.Dispose(key);
                _activeScopes.TryRemove(key, out _);
            }
            _logger.LogInformation("Cleaned up all registered scopes");
        }
    }

    /// <summary>
    /// NUnit SetUpFixture for automatic resource cleanup at assembly level.
    /// Automatically disposes all DI resources.
    /// </summary>
    [SetUpFixture]
    public class AutoDisposeSetUpFixture : ITestAction
    {
        [OneTimeSetUp]
        public void AssemblySetUp()
        {
            
        }

        [OneTimeTearDown]
        public void AssemblyTearDown()
        {
            
            AutoDisposeManager.CleanupAll();
        }

        public void BeforeTest(ITest test)
        {
            
        }

        public void AfterTest(ITest test)
        {
            AutoDisposeManager.AutoDisposeTestScope();
            
            if (test.IsSuite)
            {
                AutoDisposeManager.AutoDisposeFixtureScope();
            }
        }

        public ActionTargets Targets => ActionTargets.Test | ActionTargets.Suite;
    }
}

