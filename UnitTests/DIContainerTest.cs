using Microsoft.VisualStudio.TestTools.UnitTesting;

using DIContainer;
using System.Collections.Generic;

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
        public void TestSingleDependency()
        {
            var dependencies = new DependenciesConfiguration();
            dependencies.Register<IService, ServiceImplementation>();
            dependencies.Register<IRepository, RepositoryImplementation>();

            var provider = new DependencyProvider(dependencies);
            var service1 = provider.Resolve<IService>();

            Assert.AreEqual((service1 as ServiceImplementation).repository.IdentificateRepository(), "hi, it's a repository object");
        }

        [TestMethod]
        public void TestMultipleDependencies()
        {
            var dependencies = new DependenciesConfiguration();
            dependencies.Register<IService, ServiceImplementation>();
            dependencies.Register<IService, AnotherServiceImplementation>();
            dependencies.Register<IRepository, AnotherRespositoryImplementation>();

            var provider = new DependencyProvider(dependencies);
            var services = provider.Resolve<IEnumerable<IService>>() as IList<object>;

            Assert.AreEqual(services.Count, 2);
            Assert.AreEqual((services[0] as ServiceImplementation).repository.IdentificateRepository(), "hi, it's an another repository object");
            Assert.AreEqual((services[1] as AnotherServiceImplementation).repository.IdentificateRepository(), "hi, it's an another repository object");
        }

        [TestMethod]
        public void TestNotRegisteredDependency()
        {
            var dependencies = new DependenciesConfiguration();
            dependencies.Register<IService, ServiceImplementation>();

            var provider = new DependencyProvider(dependencies);
            var repo = provider.Resolve<IRepository>();

            Assert.AreEqual(repo, null);
        }

        [TestMethod]
        public void TestGenericDependency()
        {
            var dependencies = new DependenciesConfiguration();
            dependencies.Register<IRepository, RepositoryImplementation>();
            dependencies.Register<IService<IRepository>, ServiceImpl<IRepository>>();

            var provider = new DependencyProvider(dependencies);
            var service = provider.Resolve<IService<IRepository>>();

            Assert.AreEqual((service as ServiceImpl<IRepository>).repo.IdentificateRepository(), "hi, it's a repository object");
        }

        [TestMethod]
        public void TestOpenGenericDependency()
        {
            var dependencies = new DependenciesConfiguration();
            dependencies.Register(typeof(IService<>), typeof(ServiceImpl<>));
            dependencies.Register<IRepository, RepositoryImplementation>();

            var provider = new DependencyProvider(dependencies);
            var service = provider.Resolve<IService<IRepository>>();

            Assert.AreEqual((service as ServiceImpl<IRepository>).repo.IdentificateRepository(), "hi, it's a repository object");
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

    public class AnotherServiceImplementation : IService
    {
        public IRepository repository = null;

        public AnotherServiceImplementation(IRepository repository)
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

    public class AnotherRespositoryImplementation : IRepository
    {
        public AnotherRespositoryImplementation()
        {

        }

        public string IdentificateRepository()
        {
            return "hi, it's an another repository object";
        }
    }

    public interface IService<TRepository> where TRepository : IRepository { }

    public class ServiceImpl<TRepository> : IService<TRepository> where TRepository : IRepository
    {
        public IRepository repo = null;

        public ServiceImpl(TRepository repository)
        {
            repo = repository;
        }

        public string SayHi()
        {
            return repo.IdentificateRepository();
        }
    }
}
