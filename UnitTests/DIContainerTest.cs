using Microsoft.VisualStudio.TestTools.UnitTesting;

using DIContainer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class DIContainerTest
    {
        public static readonly string RepositoryAnswer = "hi, it's a repository object";
        public static readonly string RecursionRepositoryAnswer = "hi, it's a recursion repository object";

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

            Assert.AreEqual((service1 as ServiceImplementation).repository.IdentificateRepository(), RepositoryAnswer);
        }

        [TestMethod]
        public void TestMultipleDependencies()
        {
            var dependencies = new DependenciesConfiguration();
            dependencies.Register<IService, ServiceImplementation>();
            dependencies.Register<IService, AnotherServiceImplementation>();
            dependencies.Register<IRepository, RepositoryImplementation>();

            var provider = new DependencyProvider(dependencies);
            var services = provider.Resolve<IEnumerable<IService>>() as IList<object>;

            Assert.AreEqual(services.Count, 2);
            Assert.AreEqual((services[0] as ServiceImplementation).repository.IdentificateRepository(), RepositoryAnswer);
            Assert.AreEqual((services[1] as AnotherServiceImplementation).repository.IdentificateRepository(), RepositoryAnswer);
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
            dependencies.Register<IGenericService<IRepository>, GenericServiceImplementation<IRepository>>();
            dependencies.Register<IRepository, RepositoryImplementation>();

            var provider = new DependencyProvider(dependencies);
            var service = provider.Resolve<IGenericService<IRepository>>();

            Assert.AreEqual((service as GenericServiceImplementation<IRepository>).repo.IdentificateRepository(), RepositoryAnswer);
        }

        [TestMethod]
        public void TestOpenGenericDependency()
        {
            var dependencies = new DependenciesConfiguration();
            dependencies.Register(typeof(IGenericService<>), typeof(GenericServiceImplementation<>));
            dependencies.Register<IRepository, RepositoryImplementation>();

            var provider = new DependencyProvider(dependencies);
            var service = provider.Resolve<IGenericService<IRepository>>();

            Assert.AreEqual((service as GenericServiceImplementation<IRepository>).repo.IdentificateRepository(), RepositoryAnswer);
        }

        [TestMethod]
        public void TestInvalidDependencies()
        {
            var dependencies = new DependenciesConfiguration();
            dependencies.Register<InvalidDependencyClass, ExampleImplementationClass>();
            dependencies.Register<IService, InvalidImplementationClass>();
            dependencies.Register<IRepository, RepositoryImplementation>();

            var provider = new DependencyProvider(dependencies);
            var service = provider.Resolve<InvalidDependencyClass>();
            var service2 = provider.Resolve<IService>();

            Assert.AreEqual(service, null);
            Assert.AreEqual(service2, null);
        }

        [TestMethod]
        public void TestRecursionDependecies()
        {
            var dependencies = new DependenciesConfiguration();
            dependencies.Register<IService, ServiceImplementation>();
            dependencies.Register<IRepository, RecursionRepositoryImplementation>();
            dependencies.Register<IAnotherRepository, AnotherRepositoryImplementation>();

            var provider = new DependencyProvider(dependencies);
            var service = provider.Resolve<IService>();

            IRepository FirstRepo = (service as ServiceImplementation).repository;
            Assert.AreEqual(FirstRepo.IdentificateRepository(), RecursionRepositoryAnswer);
            Assert.IsTrue(FirstRepo is RecursionRepositoryImplementation);

            IAnotherRepository SecondRepo = (FirstRepo as RecursionRepositoryImplementation).repo;
            Assert.AreEqual(SecondRepo.IdentificateRepository(), RepositoryAnswer);
            Assert.IsTrue(SecondRepo is AnotherRepositoryImplementation);
        }

        [TestMethod]
        public void TestLifetime()
        {
            var dependencies = new DependenciesConfiguration();
            dependencies.Register<IService, ServiceImplementation>(ImplementationLifeTime.Singleton);
            dependencies.Register<IRepository, RepositoryImplementation>();

            var provider = new DependencyProvider(dependencies);
            var service1 = provider.Resolve<IService>();
            var service2 = provider.Resolve<IService>();

            Assert.IsTrue(Equals(service1, service2));

            var service3 = Task.Run(() => provider.Resolve<IService>()).Result;
            var service4 = Task.Run(() => provider.Resolve<IService>()).Result;
            var service5 = Task.Run(() => provider.Resolve<IService>()).Result;

            Assert.IsTrue(Equals(service3, service4) && 
                Equals(service4, service5) && 
                Equals(service3, service5) && 
                Equals(service3, service2) &&
                Equals(service3, service1));

            dependencies = new DependenciesConfiguration();
            dependencies.Register<IService, ServiceImplementation>(ImplementationLifeTime.InstancePerDependency);
            dependencies.Register<IRepository, RepositoryImplementation>();

            provider = new DependencyProvider(dependencies);
            service1 = provider.Resolve<IService>();
            service2 = provider.Resolve<IService>();
            service3 = provider.Resolve<IService>();

            Assert.IsFalse(Equals(service1, service2));
            Assert.IsFalse(Equals(service1, service3));
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
            return DIContainerTest.RepositoryAnswer;
        }
    }

    public interface IAnotherRepository
    {
        string IdentificateRepository();
    }

    public class RecursionRepositoryImplementation : IRepository
    {
        public IAnotherRepository repo = null;

        public RecursionRepositoryImplementation(IAnotherRepository repo)
        {
            this.repo = repo;
        }

        public string IdentificateRepository()
        {
            return DIContainerTest.RecursionRepositoryAnswer;
        }
    }

    public class AnotherRepositoryImplementation : IAnotherRepository
    {
        public AnotherRepositoryImplementation()
        {

        }

        public string IdentificateRepository()
        {
            return DIContainerTest.RepositoryAnswer;
        }
    }

    public interface IGenericService<TRepository> where TRepository : IRepository { }

    public class GenericServiceImplementation<TRepository> : IGenericService<TRepository> where TRepository : IRepository
    {
        public IRepository repo = null;

        public GenericServiceImplementation(TRepository repository)
        {
            repo = repository;
        }

        public string SayHi()
        {
            return repo.IdentificateRepository();
        }
    }

    public abstract class InvalidDependencyClass
    {
        public abstract string ExampleMethod();
    }

    public abstract class ExampleImplementationClass : InvalidDependencyClass
    {
        public IRepository repo = null;

        public ExampleImplementationClass(IRepository repo)
        {
            this.repo = repo;    
        }

        public override string ExampleMethod()
        {
            return "it is example method";
        }
    }

    public class InvalidImplementationClass : IService
    {
        public IRepository repo = null;

        private InvalidImplementationClass()
        {

        }

        public string ExampleMethod()
        {
            return "it is example method";
        }
    }
}
