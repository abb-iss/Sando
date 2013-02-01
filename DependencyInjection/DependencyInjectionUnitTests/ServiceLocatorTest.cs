using System.Threading;
using DependencyInjection;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace DependencyInjectionUnitTests
{
    [TestFixture]
    public class ServiceLocatorTest
    {
        [Test]
        public void GIVEN_RegisteredType_WHEN_ResolveMethodIsCalledTwiceWithinTheSameThread_THEN_TheSameObjectIsReturnedForEachCall()
        {
            ServiceLocator.RegisterType<IUnityContainer, UnityContainer>();
            _instance1 = ServiceLocator.Resolve<IUnityContainer>();
            _instance2 = ServiceLocator.Resolve<IUnityContainer>();
            Assert.IsTrue(ReferenceEquals(_instance1, _instance2));
        }

        [Test]
        public void GIVEN_RegisteredType_WHEN_ResolveMethodIsCalledTwiceOnceInDifferentThread_THEN_TheSameObjectIsReturnedForEachCall()
        {
            ServiceLocator.RegisterType<IUnityContainer, UnityContainer>();
            _instance1 = ServiceLocator.Resolve<IUnityContainer>();
            
            var thread = new Thread(() => Resolve(out _instance2));
            thread.Start();
            thread.Join();
            Assert.IsTrue(ReferenceEquals(_instance1, _instance2));
        }

        [Test]
        public void GIVEN_RegisteredType_WHEN_ResolveMethodIsCalledTwiceFromDifferentThreads_THEN_TheSameObjectIsReturnedForEachCall()
        {
            ServiceLocator.RegisterType<IUnityContainer, UnityContainer>();

            var thread1 = new Thread(() => Resolve(out _instance1));
            thread1.Start();
            var thread2 = new Thread(() => Resolve(out _instance2));
            thread2.Start();
            
            thread1.Join();
            thread2.Join();

            Assert.IsTrue(ReferenceEquals(_instance1, _instance2));
        }

        private void Resolve(out object instance)
        {
            instance = ServiceLocator.Resolve<IUnityContainer>();
        }

        private object _instance1;
        private object _instance2;
    }
}
