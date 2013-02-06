using System.Threading;
using Sando.DependencyInjection;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace Sando.DependencyInjectionUnitTests
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

        [Test]
        public void GIVEN_RegisteredTypeWasCalledWithDifferentNames_WHEN_ResolveMethodIsCalledWithDifferentNames_THEN_DifferentObjectIsReturnedForEachCall()
        {
            ServiceLocator.RegisterInstance<IInterf>("name1", new InterfImpl1());
            ServiceLocator.RegisterInstance<IInterf>("name2", new InterfImpl2());
            ServiceLocator.RegisterInstance<IUnityContainer>("name3", new UnityContainer());
            _instance1 = ServiceLocator.Resolve<IInterf>("name1");
            _instance2 = ServiceLocator.Resolve<IInterf>("name2");
            var instance3 = ServiceLocator.Resolve<IUnityContainer>("name3");
            Assert.IsTrue(_instance1 is InterfImpl1);
            Assert.IsTrue(_instance2 is InterfImpl2);
            Assert.IsFalse(ReferenceEquals(_instance1, _instance2));
        }

        private void Resolve(out object instance)
        {
            instance = ServiceLocator.Resolve<IUnityContainer>();
        }

        private object _instance1;
        private object _instance2;
    }

    internal interface IInterf
    {
    }

    internal class InterfImpl1 : IInterf
    {
    }

    internal class InterfImpl2 : IInterf
    {
    }
}
