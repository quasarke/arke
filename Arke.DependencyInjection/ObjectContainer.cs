using System;
using System.Diagnostics.CodeAnalysis;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Arke.DependencyInjection
{
    public class ObjectContainer
    {
        private readonly Container _container;

        [SuppressMessage("NDepend", "ND1901:AvoidNonReadOnlyStaticFields", Justification="Allows for unit testing.")]
        private static ObjectContainer _instance;
        private readonly ILifecycleConverter _lifecycleConverter;

        public ObjectContainer()
        {
            _container = new Container();
            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            _lifecycleConverter = new SimpleInjectorLifecycleConverter();
        }
        
        public static void SetTestInstance(ObjectContainer container)
        {
            _instance = container;
        }

        public virtual T GetObjectInstance<T>() where T : class 
        {
            return _container.GetInstance<T>();
        }

        public virtual object GetObjectInstance(Type objectType)
        {
            return _container.GetInstance(objectType);
        }

        public static ObjectContainer GetInstance()
        {
            return _instance ??= new ObjectContainer();
        }

        public virtual Container GetSimpleInjectorContainer()
        {
            return _container;
        }

        public virtual void Verify()
        {
            _container.Verify();
        }

        public virtual void RegisterSingleton<T>(Func<T> objectCreator) where T : class
        {
            _container.RegisterSingleton(objectCreator);
        }

        public virtual void RegisterSingleton<T>(T objectCreator) where T : class
        {
            _container.RegisterInstance<T>(objectCreator);
        }
        
        public virtual void Register<TInterface, TImplementation>() where TInterface : class where TImplementation : class, TInterface
        {
            _container.Register<TInterface, TImplementation>();
        }

        public virtual void Register<TInterface, TImplementation>(ObjectLifecycle lifecycle) where TInterface : class where TImplementation : class, TInterface
        {
            _container.Register<TInterface, TImplementation>((Lifestyle)_lifecycleConverter.GetContainerSpecificLifecycle(lifecycle));
        }

        public virtual void Register<T>(Func<T> p) where T : class
        {
            _container.Register(p);
        }
    }
}
