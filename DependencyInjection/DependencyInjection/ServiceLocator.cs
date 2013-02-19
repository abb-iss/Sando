using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Microsoft.Practices.Unity;

namespace Sando.DependencyInjection
{
    public static class ServiceLocator
    {
        static ServiceLocator()
        {
            UnityContainers = new Dictionary<int, IUnityContainer>();
        }

        public static void RegisterType<TFrom, TTo>() where TTo : TFrom
        {
            CurrentUnityContainer.RegisterType(typeof(TFrom), typeof(TTo), null, new HierarchicalLifetimeManager());
        }

        public static void RegisterType<TFrom, TTo>(string name) where TTo : TFrom
        {
            CurrentUnityContainer.RegisterType(typeof(TFrom), typeof(TTo), name, new HierarchicalLifetimeManager());
        }

        public static void RegisterInstance<TInterface>(TInterface instance)
        {
            CurrentUnityContainer.RegisterInstance(typeof(TInterface), null, instance, new HierarchicalLifetimeManager());
        }

        public static void RegisterInstance<TInterface>(string name, TInterface instance)
        {
            CurrentUnityContainer.RegisterInstance(typeof(TInterface), name, instance, new HierarchicalLifetimeManager());
        }

        public static T Resolve<T>() where T : class
        {
            Contract.Ensures(Contract.Result<T>() != null);
            
            T service = CurrentUnityContainer.Resolve<T>();

            Contract.Assert(service != null);

            return service;
        }

        public static T Resolve<T>(string name) where T : class
        {
            Contract.Ensures(Contract.Result<T>() != null);

            T service = CurrentUnityContainer.Resolve<T>(name);

            Contract.Assert(service != null);

            return service;
        }

        public static T ResolveOptional<T>() where T : class
        {
            return CurrentUnityContainer.IsRegistered<T>() ? CurrentUnityContainer.Resolve<T>() : null;
        }

        public static T ResolveOptional<T>(string name) where T : class
        {
            return CurrentUnityContainer.IsRegistered<T>(name) ? CurrentUnityContainer.Resolve<T>(name) : null;
        }

        public static void ClearAllRegistrations()
        {
            var currentProcessId = Process.GetCurrentProcess().Id;
            if (UnityContainers.ContainsKey(currentProcessId))
                UnityContainers[currentProcessId] = new UnityContainer();
        }

        private static IUnityContainer CurrentUnityContainer
        {
            get
            {
                var currentProcessId = Process.GetCurrentProcess().Id;
                if (!UnityContainers.ContainsKey(currentProcessId))
                    UnityContainers.Add(currentProcessId, new UnityContainer());

                return UnityContainers[currentProcessId];
            }
        }

        private static readonly Dictionary<int, IUnityContainer> UnityContainers;
    }
}