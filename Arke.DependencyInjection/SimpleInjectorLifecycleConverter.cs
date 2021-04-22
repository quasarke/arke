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
            return lifecycle switch
            {
                ObjectLifecycle.Singleton => Lifestyle.Singleton,
                ObjectLifecycle.Transient => Lifestyle.Transient,
                _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null),
            };
        }
    }
}
