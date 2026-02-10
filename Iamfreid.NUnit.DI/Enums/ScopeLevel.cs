namespace Iamfreid.NUnit.DI.Enums
{
    /// <summary>
    /// Defines the scope level for dependency injection container in NUnit tests.
    /// </summary>
    public enum ScopeLevel
    {
        /// <summary>
        /// A new scope is created for each test method.
        /// </summary>
        Test = 0,

        /// <summary>
        /// One scope is created for the entire test fixture.
        /// </summary>
        Fixture = 1,

        /// <summary>
        /// One scope is created for all tests across all assemblies.
        /// </summary>
        Global = 2
    }
}




