using System;
using System.Collections.Generic;

namespace DIContainer
{
    /*
     * TDependency - any reference type
     * TImplementation - non abstract class compatible with TDependency
     */
    public class DependenciesConfiguration
    {
        internal List<(Type dependency, Type implementation)> config = new List<(Type, Type)>();

        public void Register<TDependency, TImplementation>()
        {
            var cfgTuple = (typeof(TDependency), typeof(TImplementation));
            if (!config.Exists(tuple => Equals(tuple, cfgTuple)))
                config.Add(cfgTuple);
        }
    }
}
