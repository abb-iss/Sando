using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Sando.Core.Tools;

namespace Sando.Core.UnitTests.Tools
{
    [TestFixture]
    public class TimedProcessorTests
    {
        private readonly TimedProcessor processor;
        private readonly object locker = new object();
        private int fieldToUpdate = 0;


        public TimedProcessorTests()
        {
            this.processor = TimedProcessor.GetInstance();
        }


        private void incrementField()
        {
            lock (locker)
            {
                fieldToUpdate++;
            }
        }

        private void decrementField()
        {
            lock (locker)
            {
                fieldToUpdate--;
            }
        }

        private void EnsureFieldCondition(Predicate<int> condition)
        {
            lock (locker)
            {
                Assert.IsTrue(condition.Invoke(fieldToUpdate));
            }
        }


        [Test]
        public void AddOneTask()
        {
            processor.AddTimedTask(incrementField, 10);
            Thread.Sleep(1000);
            EnsureFieldCondition(i => i > 50);
            EnsureFieldCondition(i => i < 100);
            processor.RemoveTimedTask(incrementField);
        }

        [Test]
        public void AddTwoTasks()
        {
            processor.AddTimedTask(incrementField, 5);
            processor.AddTimedTask(decrementField, 10);
            Thread.Sleep(1000);
            EnsureFieldCondition(i => i > 50);
            EnsureFieldCondition(i => i < 100);
            processor.RemoveTimedTask(incrementField);
            processor.RemoveTimedTask(decrementField);
        }

    }
}
