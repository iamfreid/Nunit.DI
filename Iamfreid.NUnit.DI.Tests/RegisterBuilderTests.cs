using Iamfreid.NUnit.DI.StaticApi;
using Serilog;
using Serilog.Extensions.Logging;

namespace Iamfreid.NUnit.DI.Tests
{
    [TestFixture]
    public class RegisterBuilderTests
    {
        [OneTimeSetUp]
        public void FixtureSetup()
        {
            // Настройка Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
    
            var loggerFactory = new SerilogLoggerFactory(Log.Logger);
            UnitDI.SetLoggerFactory(loggerFactory);
        }

        [Test]
        public void Register_Fixture_WithBuilder_RegistersAndResolvesServices()
        {
            UnitDI.Register.Fixture<RegisterBuilderTests>()
                .AddSingleton<ITestService, TestService>()
                .Build();

            var service = UnitDI.Resolve.Fixture<RegisterBuilderTests>().Required<ITestService>();
            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.InstanceOf<TestService>());
            Assert.That(service.GetValue(), Is.EqualTo("Test Service"));
            
            UnitDI.Register.Fixture<RegisterBuilderTests>()
                .AddSingleton<ITestService, TestService>()
                .Build();
        }

        [Test]
        public void Register_Fixture_WithInstance_RegistersInstance()
        {
            var instance = new TestService();

            UnitDI.Register.Fixture<RegisterBuilderTests>()
                .AddInstance<ITestService>(instance)
                .Build();

            var resolved = UnitDI.Resolve.Fixture<RegisterBuilderTests>().Required<ITestService>();
            Assert.That(resolved, Is.SameAs(instance));
        }

        [Test]
        public void Register_Fixture_WithFactory_RegistersFactory()
        {
            UnitDI.Register.Fixture<RegisterBuilderTests>()
                .AddFactory<ITestService>(sp => new TestService())
                .Build();

            var service1 = UnitDI.Resolve.Fixture<RegisterBuilderTests>().Required<ITestService>();
            var service2 = UnitDI.Resolve.Fixture<RegisterBuilderTests>().Required<ITestService>();
            
            Assert.That(service1, Is.Not.Null);
            Assert.That(service2, Is.Not.Null);
            Assert.That(service1, Is.SameAs(service2));
        }

        [Test]
        public void Register_Test_WithBuilder_RegistersForTestScope()
        {
            UnitDI.Register.Test<RegisterBuilderTests>()
                .AddSingleton<ITestService, TestService>()
                .Build();

            var service = UnitDI.Resolve.Test<RegisterBuilderTests>().Required<ITestService>();
            Assert.That(service, Is.Not.Null);
        }

        [Test]
        public void Register_Global_WithBuilder_RegistersForGlobalScope()
        {
            UnitDI.Register.Global()
                .AddSingleton<ITestService, TestService>()
                .Build();

            var service = UnitDI.Resolve.Global().Required<ITestService>();
            Assert.That(service, Is.Not.Null);
        }



        [Test]
        public void Register_Fixture_WithChainedCalls_RegistersAllServices()
        {
            UnitDI.Register.Fixture<RegisterBuilderTests>()
                .AddSingleton<ITestService, TestService>()
                .AddInstance<IComprehensiveTestService>(new ComprehensiveTestService())
                .Build();

            var testService = UnitDI.Resolve.Fixture<RegisterBuilderTests>().Required<ITestService>();
            var comprehensiveService = UnitDI.Resolve.Fixture<RegisterBuilderTests>().Required<IComprehensiveTestService>();

            Assert.That(testService, Is.Not.Null);
            Assert.That(comprehensiveService, Is.Not.Null);
        }

        [Test]
        public void Register_Global_WithTransient_RegistersTransientService()
        {
            UnitDI.Register.Global()
                .AddSingleton<ITestService, TestService>()
                .AddTransient<ITransientService, TransientService>()
                .Build();

            var service1 = UnitDI.Resolve.Global().Required<ITransientService>();
            var service2 = UnitDI.Resolve.Global().Required<ITransientService>();

            Assert.That(service1, Is.Not.Null);
            Assert.That(service2, Is.Not.Null);
            Assert.That(service1, Is.Not.SameAs(service2));
        }
    }

    [TestFixture]
    [Order(1)]
    public class FirstFixtureForGlobalTest
    {
        [OneTimeSetUp]
        public void FixtureSetup()
        {
            UnitDI.Register.Global()
                .AddSingleton<ITestService, TestService>()
                .Build();
        }

        [Test]
        [Order(1)]
        public void FirstFixture_CanResolveGlobalService()
        {
            var service = UnitDI.Resolve.Global().Required<ITestService>();
            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.InstanceOf<TestService>());
        }
    }

    [TestFixture]
    [Order(2)]
    public class SecondFixtureForGlobalTest
    {
        [Test]
        [Order(1)]
        public void SecondFixture_CanResolveGlobalServiceFromAnotherFixture()
        {
            var service = UnitDI.Resolve.Global().Required<ITestService>();
            
            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.InstanceOf<TestService>());
            
            var serviceFromFirstFixture = UnitDI.Resolve.Global().Required<ITestService>();
            Assert.That(service, Is.SameAs(serviceFromFirstFixture), 
                "Global Singleton service should be the same instance across different fixtures");
        }

        [Test]
        [Order(2)]
        public void SecondFixture_GlobalServiceIsSingletonAcrossFixtures()
        {
            var service1 = UnitDI.Resolve.Global().Required<ITestService>();
            var service2 = UnitDI.Resolve.Global().Required<ITestService>();
            
            Assert.That(service1, Is.SameAs(service2), 
                "Global Singleton service should return the same instance on multiple resolves");
        }
    }
}
