using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DIContainer
{
    /*
     * TDependency - any reference type
     * TImplementation - non abstract class compatible with TDependency
     */
    public class DependenciesConfiguration
    {
        internal Dictionary<Type, List<ImplementationInfo>> config = new Dictionary<Type, List<ImplementationInfo>>();

        public void Register<TDependency, TImplementation>(ImplementationLifeTime lifeTime = ImplementationLifeTime.InstancePerDependency)
            where TDependency : class
            where TImplementation : TDependency
        {
            Register(typeof(TDependency), typeof(TImplementation), lifeTime);
        }

        public void Register(Type DependencyType, Type ImplementationType, ImplementationLifeTime lifeTime = ImplementationLifeTime.InstancePerDependency)
        {
            if (!isDependencyValid(DependencyType, ImplementationType)) return;
            ImplementationInfo implInfo = new ImplementationInfo(ImplementationType, lifeTime);

            if (!config.ContainsKey(DependencyType))
                config.Add(DependencyType, new List<ImplementationInfo> { implInfo });
            else if (!config[DependencyType].Exists(obj => Equals(obj.ImplementationType, implInfo.ImplementationType)))
                config[DependencyType].Add(implInfo);
        }

        private bool isDependencyValid(Type DependencyType, Type ImplementationType)
        {
            return !ImplementationType.IsAbstract &&
                    ImplementationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Any() &&
                    (DependencyType.IsAssignableFrom(ImplementationType) || DependencyType.IsGenericTypeDefinition);
        }
    }
}
