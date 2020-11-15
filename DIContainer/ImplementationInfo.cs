using System;
using System.Collections.Generic;
using System.Text;

namespace DIContainer
{
    public class ImplementationInfo
    {
        internal Type ImplementationType;
        internal ImplementationLifeTime LifeTime;

        public ImplementationInfo(Type type, ImplementationLifeTime lf)
        {
            ImplementationType = type;
            LifeTime = lf;
        }
    }

    public enum ImplementationLifeTime
    {
        Singleton, InstancePerDependency
    }
}
