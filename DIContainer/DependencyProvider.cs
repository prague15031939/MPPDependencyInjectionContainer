using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DIContainer
{
    public class DependencyProvider
    {
        internal DependenciesConfiguration dependencies;
        internal ConcurrentDictionary<Type, object> ImplementationInstances = new ConcurrentDictionary<Type, object>();

        public DependencyProvider(DependenciesConfiguration dependencies)
        {
            this.dependencies = dependencies;
        }

        public TDependency Resolve<TDependency>()
        {
            return (TDependency)ResolveByType(typeof(TDependency));
        }

        private object ResolveByType(Type DependencyType)
        {
            // passing IEnumerable
            if (typeof(IEnumerable).IsAssignableFrom(DependencyType))
            {
                Type RealDependencyType = DependencyType.GetGenericArguments()[0];
                IList ResultObjectList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(RealDependencyType));
                if (dependencies.config.ContainsKey(RealDependencyType))
                {
                    foreach (var item in dependencies.config[RealDependencyType])
                        ResultObjectList.Add(CreateImplementation(item));
                    return ResultObjectList;
                }
                else
                    return null;
            }

            // open generic dependency
            if (isGenericDependency(DependencyType))
            {
                Type GenericType = DependencyType.GetGenericTypeDefinition();
                if (dependencies.config.ContainsKey(GenericType))
                {
                    ImplementationInfo implInfo = dependencies.config[GenericType].First();
                    implInfo.ImplementationType = implInfo.ImplementationType.MakeGenericType(DependencyType.GetGenericArguments()[0]);
                    return CreateImplementation(implInfo);
                }
            }

            // simple or generic dependency
            if (dependencies.config.ContainsKey(DependencyType))
                return CreateImplementation(dependencies.config[DependencyType].First());

            return null;
        }

        private object CreateImplementation(ImplementationInfo implInfo)
        {
            Type ImplementationType = implInfo.ImplementationType;
            if (implInfo.LifeTime == ImplementationLifeTime.Singleton && ImplementationInstances.ContainsKey(ImplementationType))
                return ImplementationInstances[ImplementationType];

            var CtorParams = new List<object>();
            ConstructorInfo DesiredCtor = ImplementationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).First();
            foreach (ParameterInfo parameter in DesiredCtor.GetParameters())
                CtorParams.Add(ResolveByType(parameter.ParameterType));

            object ResultObject = Activator.CreateInstance(ImplementationType, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, CtorParams.ToArray(), null);
            if (implInfo.LifeTime == ImplementationLifeTime.Singleton && !ImplementationInstances.ContainsKey(ImplementationType)) 
                if (!ImplementationInstances.TryAdd(ImplementationType, ResultObject))
                    return ImplementationInstances[ImplementationType];

            return ResultObject;
        }

        public static bool isGenericDependency(Type t)
        {
            return t.IsGenericType;
        }
    }
}
