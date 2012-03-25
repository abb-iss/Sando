using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace Sando.UI.Monitoring
{
	public class BackgroundWorkersManager<T>
	{
		public BackgroundWorkersManager(int maxNrOfParallelWorkers, BackgroundWorker queueEmptyEventWorker)
		{
			this.maxNrOfParallelWorkers = maxNrOfParallelWorkers;
			this.numberOfCurrentlyRunningWorkers = 0;

			this.queueEmptyEventWorker = queueEmptyEventWorker;

			this.workersWithArguments = new Queue<Tuple<BackgroundWorker, T>>();

			workersRunner = new Thread(new ThreadStart(ProcessQueue));
			workersRunner.Start();
		}

		public void AddWorker(BackgroundWorker worker, T argument)
		{
			worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorkerCompleted);
			lock(workersWithArguments)
			{
				workersWithArguments.Enqueue(new Tuple<BackgroundWorker, T>(worker, argument));
			}		
		}

		public int GetNumberOfCurrentlyRunningWorkers()
		{
			lock(workersWithArguments)
			{
				return this.numberOfCurrentlyRunningWorkers;
			}
		}

		private void ProcessQueue()
		{
			while(true)
			{
				lock(workersWithArguments)
				{
					if(workersWithArguments.Count > 0 && numberOfCurrentlyRunningWorkers < maxNrOfParallelWorkers)
					{
						++numberOfCurrentlyRunningWorkers;
						Tuple<BackgroundWorker, T> currentWorkerWithArgument = workersWithArguments.Dequeue();
						BackgroundWorker currentWorker = currentWorkerWithArgument.Item1;
						currentWorker.RunWorkerAsync(currentWorkerWithArgument.Item2);
					}
				}
			}
		}

		private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			lock(workersWithArguments)
			{
				--numberOfCurrentlyRunningWorkers;
				if(numberOfCurrentlyRunningWorkers == 0)
					queueEmptyEventWorker.RunWorkerAsync();
			}
		}

		~BackgroundWorkersManager()
		{
			lock(workersWithArguments)
			{
				workersWithArguments.Clear();
			}
			workersRunner.Abort();
		}

		private Thread workersRunner;
		private Queue<Tuple<BackgroundWorker, T>> workersWithArguments;
		private int maxNrOfParallelWorkers;
		private int numberOfCurrentlyRunningWorkers;
		private BackgroundWorker queueEmptyEventWorker;
	}
}
