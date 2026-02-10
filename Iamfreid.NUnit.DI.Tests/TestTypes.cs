namespace Iamfreid.NUnit.DI.Tests
{
    public interface ITestService
    {
        string GetValue();
    }

    public class TestService : ITestService
    {
        public string GetValue() => "Test Service";
    }

    public interface IScopedService
    {
        string GetValue();
    }

    public class ScopedService : IScopedService
    {
        public string GetValue() => "Scoped Service";
    }

    public interface ITransientService
    {
        string GetValue();
    }

    public class TransientService : ITransientService
    {
        public string GetValue() => "Transient Service";
    }

    public interface IComprehensiveTestService
    {
        string GetValue();
    }

    public class ComprehensiveTestService : IComprehensiveTestService
    {
        public string GetValue() => "Comprehensive Test Service";
    }
}
