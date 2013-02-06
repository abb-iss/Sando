using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using Microsoft.Practices.Unity;

namespace Sando.DependencyInjection
{
    public static class ServiceLocator
    {
        static ServiceLocator()
        {
            UnityContainers = new Dictionary<int, IUnityContainer>();
        }

        public static IUnityContainer RegisterType<TFrom, TTo>(params InjectionMember[] injectionMembers) where TTo : TFrom
        {
            return CurrentUnityContainer.RegisterType(typeof(TFrom), typeof(TTo), null, new ContainerControlledLifetimeManager(), injectionMembers);
        }

        public static IUnityContainer RegisterType<TFrom, TTo>(string name, params InjectionMember[] injectionMembers) where TTo : TFrom
        {
            return CurrentUnityContainer.RegisterType(typeof(TFrom), typeof(TTo), name, new ContainerControlledLifetimeManager(), injectionMembers);
        }

        public static IUnityContainer RegisterInstance<TInterface>(TInterface instance)
        {
            return CurrentUnityContainer.RegisterInstance(typeof(TInterface), null, instance, new ContainerControlledLifetimeManager());
        }

        public static IUnityContainer RegisterInstance<TInterface>(string name, TInterface instance)
        {
            return CurrentUnityContainer.RegisterInstance(typeof(TInterface), name, instance, new ContainerControlledLifetimeManager());
        }

        public static T Resolve<T>() where T : class
        {
            Contract.Ensures(Contract.Result<T>() != null);
            
            T service;
            var resolved = TryResolve(out service);

            Contract.Assert(resolved, String.Format(CultureInfo.InvariantCulture, "Unable to resolve service of type {0} using current resolver.", typeof(T)));
            Contract.Assert(service != null);

            return service;
        }

        public static T Resolve<T>(string name) where T : class
        {
            Contract.Ensures(Contract.Result<T>() != null);

            T service;
            var resolved = TryResolve(name, out service);

            Contract.Assert(resolved, String.Format(CultureInfo.InvariantCulture, "Unable to resolve service of type {0} using current resolver.", typeof(T)));
            Contract.Assert(service != null);

            return service;
        }

        public static T ResolveOptional<T>() where T : class
        {
            T service;
            return TryResolve(out service) ? service : null;
        }

        public static T ResolveOptional<T>(string name) where T : class
        {
            T service;
            return TryResolve(name, out service) ? service : null;
        }

        private static bool TryResolve<T>(out T service) where T : class
        {
            service = CurrentUnityContainer.Resolve<T>();
            
            if (service != null)
                return true;

            return false;
        }

        private static bool TryResolve<T>(string name, out T service) where T : class
        {
            service = CurrentUnityContainer.Resolve<T>(name);

            if (service != null)
                return true;

            return false;
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