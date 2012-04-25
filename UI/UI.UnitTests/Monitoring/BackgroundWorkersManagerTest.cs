using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.ComponentModel;
using System.Threading;
using Sando.UI.Monitoring;

namespace Sando.UI.UnitTests.Monitoring
{
	[TestFixture]
	public class BackgroundWorkersManagerTest
	{
		//[Test]
		//public void BackgroundWorkersManager_RunsOnlyGivenNumberOfWorkersAtOneTime()
		//{
		//    int totalBackgroundWorkers = 11;
		//    int maxNrOfParallelWorkers = 5;

		//    BackgroundWorker queueEmptyEventWorker = new BackgroundWorker();
		//    queueEmptyEventWorker.DoWork += new DoWorkEventHandler(queueEmptyEventWorker_DoWork);
		//    BackgroundWorkersManager<int> backgroundWorkersManager = new BackgroundWorkersManager<int>(maxNrOfParallelWorkers, queueEmptyEventWorker);
		//    for(int i = 0; i < totalBackgroundWorkers; ++i)
		//    {
		//        BackgroundWorker worker = new BackgroundWorker();
		//        worker.DoWork += new DoWorkEventHandler(worker_DoWork);
		//        backgroundWorkersManager.AddWorker(worker, i);
		//        Thread.Sleep(100);
		//    }
		//    Assert.True(backgroundWorkersManager.GetNumberOfCurrentlyRunningWorkers() <= maxNrOfParallelWorkers, "BackgroundWorkersManager currently running workers number too high!");
		//    while(counter < totalBackgroundWorkers)
		//    {
		//    }
		//    Thread.Sleep(1000);
		//    Assert.True(backgroundWorkersManager.GetNumberOfCurrentlyRunningWorkers() == 0, "BackgroundWorkersManager currently running workers does not equal 0!");
		//    if(!allElementsProcessed)
		//        Assert.Fail("queueEmptyEventWorker have not been called!");
		//}

		void queueEmptyEventWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			allElementsProcessed = true;
		}

		private void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			Thread.Sleep(2000);
			++counter;
		}

		[SetUp]
		[TearDown]
		public void PrepareCounter()
		{
			counter = 0;
		}

		private int counter;
		private bool allElementsProcessed;
	}
}
