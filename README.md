# Iamfreid.NUnit.DI

Library provides a static interface and Fluent API for using dependency injection in NUnit tests.
Service registration is available at Fixture, Test, or Global levels.

Some library functions are written for highly specialized tasks. However, for solving 99.99% of tasks, you can use the Global level.

## Installation

```bash
dotnet add package Iamfreid.NUnit.DI --version 'latest'
```

## Usage

```csharp
using Iamfreid.NUnit.DI.StaticApi;

[SetUpFixture]
public class TestSetup
{
    [OneTimeSetUp]
    public void Setup()
    {
        // Register global services (available in all tests)
        // When resolving IGlobalService dependency anywhere in the assembly, the same instance of GlobalService will be obtained
        UnitDI.Register.Global()
            .AddSingleton<IGlobalService, GlobalService>()
            .Build();

        // Register fixture services (available in all tests of a specific fixture)
        // When resolving IFixtureService dependency in CoreServiceTests, the same instance of FixtureService will be obtained in each test
        UnitDI.Register.Fixture<CoreServiceTests>()
            .AddSingleton<IFixtureService, FixtureService>()
            .Build();
    }
}

[TestFixture]
public class CoreServiceTests
{
    [OneTimeSetUp]
    public void Setup()
    {
        // Register test-level service
        // When resolving dependency, a new instance of TestService will be obtained for each test in CoreServiceTests
        UnitDI.Register.Test<CoreServiceTests>()
            .AddSingleton<ITestService, TestService>()
            .Build();
    }

    [Test]
    public void TestWithGlobalService()
    {
        // Use global service
        var globalService = UnitDI.Resolve.Global().Required<IGlobalService>();
    }

    [Test]
    public void TestWithFixtureService()
    {
        // Use fixture-level service
        var fixtureService = UnitDI.Resolve.Fixture<CoreServiceTests>().Required<IFixtureService>();
    }

    [Test]
    public void TestWithTestScope()
    {
        // Use test-level service
        var testService = UnitDI.Resolve.Test<CoreServiceTests>().Required<ITestService>();
    }
}
```

## License

See [LICENSE](LICENSE) file
