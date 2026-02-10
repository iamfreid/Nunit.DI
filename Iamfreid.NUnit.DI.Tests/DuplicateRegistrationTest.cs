using Iamfreid.NUnit.DI.StaticApi;
using Microsoft.Extensions.DependencyInjection;

namespace Iamfreid.NUnit.DI.Tests
{
    [TestFixture]
    public class DuplicateRegistrationTest
    {
        [Test]
        public void DuplicateRegistration_CurrentBehavior_AllowsMultipleRegistrations()
        {
            UnitDI.Register.Fixture<DuplicateRegistrationTest>()
                .AddSingleton<ITestService, TestService>()
                .AddSingleton<ITestService, TestService>()
                .Build();

            var service = UnitDI.Resolve.Fixture<DuplicateRegistrationTest>().Required<ITestService>();
            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.InstanceOf<TestService>());
        }

        [Test]
        public void DuplicateRegistration_MicrosoftDI_Behavior_AllowsMultiple()
        {
            var services = new ServiceCollection();
            
            services.AddSingleton<ITestService, TestService>();
            services.AddSingleton<ITestService, TestService>();
            
            var provider = services.BuildServiceProvider();
            
            var service = provider.GetService<ITestService>();
            Assert.That(service, Is.Not.Null);
            
            var allServices = provider.GetServices<ITestService>();
            Assert.That(allServices, Is.Not.Null);
        }

        [Test]
        public void DuplicateRegistration_WithDifferentImplementations_LastOneWins()
        {
            var firstService = new TestService();
            var secondService = new TestService();

            UnitDI.Register.Fixture<DuplicateRegistrationTest>()
                .AddInstance<ITestService>(firstService)
                .AddInstance<ITestService>(secondService)
                .Build();

            var resolved = UnitDI.Resolve.Fixture<DuplicateRegistrationTest>().Required<ITestService>();
            Assert.That(resolved, Is.SameAs(secondService));
            Assert.That(resolved, Is.Not.SameAs(firstService));
        }
    }
}
