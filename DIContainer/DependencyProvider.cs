using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DIContainer
{
    public class DependencyProvider
    {
        internal DependenciesConfiguration dependencies;

        public DependencyProvider(DependenciesConfiguration dependencies)
        {
            this.dependencies = dependencies;
        }

        public object Resolve<TDependency>()
        {
            return ResolveByType(typeof(TDependency));
        }

        private object ResolveByType(Type DependencyType)
        {
            Type ImplementationType;
            var cfgTuple = dependencies.config.Single(cfgTuple => Equals(cfgTuple.dependency, DependencyType));
            if (cfgTuple.ToTuple() != null)
                ImplementationType = cfgTuple.implementation;
            else
                return null;

            var CtorParams = new List<object>();
            ConstructorInfo DesiredCtor = ImplementationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)[0];
            foreach (ParameterInfo parameter in DesiredCtor.GetParameters())
                CtorParams.Add(ResolveByType(parameter.ParameterType));

            return Activator.CreateInstance(ImplementationType, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, CtorParams.ToArray(), null);
        }
    }
}
