using Microsoft.VisualStudio.TestTools.UnitTesting;

using DIContainer;

namespace UnitTests
{
    [TestClass]
    public class DIContainerTest
    {
        [TestInitialize]
        public void Setup()
        {

        }

        [TestMethod]
        public void Test1()
        {
            var dependencies = new DependenciesConfiguration();
            dependencies.Register<IService, ServiceImplementation>();
            dependencies.Register<IRepository, RepositoryImplementation>();

            var provider = new DependencyProvider(dependencies);
            var service1 = provider.Resolve<IService>();

            Assert.AreEqual((service1 as ServiceImplementation).repository.IdentificateRepository(), "hi, it's a repository object");
        }
    }

    public interface IService { }

    public class ServiceImplementation : IService
    {
        public IRepository repository = null;

        public ServiceImplementation(IRepository repository)
        {
            this.repository = repository;
        }
    }

    public interface IRepository
    { 
       string IdentificateRepository();
    }
       

    public class RepositoryImplementation : IRepository
    {
        public RepositoryImplementation()
        {

        }

        public string IdentificateRepository()
        {
            return "hi, it's a repository object";
        }
    }
}
