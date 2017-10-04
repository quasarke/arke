using System;
using SimpleInjector;

namespace Arke.DependencyInjection
{
    public interface ILifecycleConverter
    {
        object GetContainerSpecificLifecycle(ObjectLifecycle lifecycle);
    }

    public class SimpleInjectorLifecycleConverter : ILifecycleConverter
    {
        public object GetContainerSpecificLifecycle(ObjectLifecycle lifecycle)
        {
            switch (lifecycle)
            {
                case ObjectLifecycle.Singleton:
                    return Lifestyle.Singleton;
                case ObjectLifecycle.Transient:
                    return Lifestyle.Transient;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null);
            }
        }
    }
}
