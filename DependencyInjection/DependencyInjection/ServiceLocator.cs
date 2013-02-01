using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using Microsoft.Practices.Unity;

namespace DependencyInjection
{
    public static class ServiceLocator
    {
        static ServiceLocator()
        {
            UnityContainer = new UnityContainer();
        }

        public static IUnityContainer RegisterType<TFrom, TTo>(params InjectionMember[] injectionMembers) where TTo : TFrom
        {
            return UnityContainer.RegisterType(typeof(TFrom), typeof(TTo), null, new PerProcessLifetimeManager(), injectionMembers);
        }

        public static IUnityContainer RegisterInstance<TInterface>(this IUnityContainer container, TInterface instance)
        {
            return container.RegisterInstance(typeof(TInterface), null, instance, new PerProcessLifetimeManager());
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

        public static T ResolveOptional<T>() where T : class
        {
            T service;
            return TryResolve(out service) ? service : null;
        }

        private static bool TryResolve<T>(out T service) where T : class
        {
            service = UnityContainer.Resolve<T>();
            
            if (service != null)
                return true;

            return false;
        }

        private static readonly IUnityContainer UnityContainer;
    }
}