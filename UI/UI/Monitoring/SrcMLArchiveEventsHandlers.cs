using System;
using System.IO;
using ABB.SrcML;
using ABB.SrcML.VisualStudio.SrcMLService;
using Sando.Core.Extensions;
using Sando.Core.Logging;
using Sando.DependencyInjection;
using Sando.Indexer;
using Sando.Indexer.IndexFiltering;
using Sando.Recommender;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Xml.Linq;
using Sando.Core.Logging.Events;
using System.Linq;
using Sando.UI.View;
using Sando.ExtensionContracts.TaskFactoryContracts;
using System.Diagnostics;
using System.ComponentModel;


namespace Sando.UI.Monitoring
{
    public class SrcMLArchiveEventsHandlers : ITaskScheduler
    {

        private ConcurrentBag<Task> tasks = new ConcurrentBag<Task>();
        private ConcurrentBag<CancellationTokenSource> cancellers = new ConcurrentBag<CancellationTokenSource>();
        private TaskScheduler scheduler;
        public TaskFactory factory;
        public static SrcMLArchiveEventsHandlers Instance;
        System.Timers.Timer hideProgressBarTimer = new System.Timers.Timer(500);

        public SrcMLArchiveEventsHandlers()
        {
            scheduler = new LimitedConcurrencyLevelTaskScheduler(2,this);
            factory = new TaskFactory(scheduler);
            Instance = this;
            hideProgressBarTimer.Elapsed += waitToUpdateProgressBar_Elapsed;
        }

   

     

        public void SourceFileChanged(object sender, FileEventRaisedArgs args)
        {
            SourceFileChanged(sender, args, false);
        }

        public int TaskCount()
        {
            lock (tasksTrackerLock)
                return tasks.Count;
        }

        public void WaitForIndexing()
        {
            while ((scheduler as LimitedConcurrencyLevelTaskScheduler).GetTasks()>0)
            {
                Thread.Sleep(500);
            }
        }

        public Task StartNew(Action a, CancellationTokenSource c)
        {
            var task = factory.StartNew(a, c.Token);
            lock (tasksTrackerLock)
            {
                tasks.Add(task);
                cancellers.Add(c);
            }
            task.ContinueWith(removeTask => RemoveTask(task, c));
            return task;
        }      
        public void SourceFileChanged(object sender, FileEventRaisedArgs args, bool commitImmediately = false)
        {
            var cancelTokenSource = new CancellationTokenSource();
            var cancelToken = cancelTokenSource.Token;            
            Action action =  () =>
            {
                cancelToken.ThrowIfCancellationRequested();

                // Ignore files that can not be indexed by Sando.
                var fileExtension = Path.GetExtension(args.FilePath);
                if (fileExtension != null && !fileExtension.Equals(String.Empty))
                {                    
                    string sourceFilePath = args.FilePath;
                    string oldSourceFilePath = args.OldFilePath;
                    var documentIndexer = ServiceLocator.Resolve<DocumentIndexer>();
                    if (ServiceLocator.Resolve<IndexFilterManager>().ShouldFileBeIndexed(args.FilePath))
                    {
                        if (ExtensionPointsRepository.Instance.GetParserImplementation(fileExtension) != null)
                        {
                            // Get SrcMLService and use its API to get the XElement
                            var srcMLService = (sender as ISrcMLGlobalService);
                            cancelToken.ThrowIfCancellationRequested();
                            var xelement = GetXElementForFile(args, srcMLService);
                            var indexUpdateManager = ServiceLocator.Resolve<IndexUpdateManager>();
                            var elapsed = lastTime - DateTime.Now;
                            if (FileEventType.FileDeleted==args.EventType || !lastFile.Equals(sourceFilePath.ToLowerInvariant()) || elapsed.TotalMilliseconds > 1000)
                            {
                                switch (args.EventType)
                                {
                                    case FileEventType.FileAdded:
                                        documentIndexer.DeleteDocuments(sourceFilePath.ToLowerInvariant());    //"just to be safe!"
                                        indexUpdateManager.Update(sourceFilePath.ToLowerInvariant(), xelement);
                                        SwumManager.Instance.AddSourceFile(sourceFilePath.ToLowerInvariant(), xelement);
                                        break;
                                    case FileEventType.FileChanged:


                                        documentIndexer.DeleteDocuments(sourceFilePath.ToLowerInvariant());
                                        indexUpdateManager.Update(sourceFilePath.ToLowerInvariant(), xelement);
                                        SwumManager.Instance.UpdateSourceFile(sourceFilePath.ToLowerInvariant(), xelement);
                                        break;
                                    case FileEventType.FileDeleted:
                                        documentIndexer.DeleteDocuments(sourceFilePath.ToLowerInvariant(), commitImmediately);
                                        SwumManager.Instance.RemoveSourceFile(sourceFilePath.ToLowerInvariant());
                                        break;
                                    case FileEventType.FileRenamed: // FileRenamed is repurposed. Now means you may already know about it, so check and only parse if not existing
                                        if (!SwumManager.Instance.ContainsFile(sourceFilePath.ToLowerInvariant()))
                                        {
                                            documentIndexer.DeleteDocuments(sourceFilePath.ToLowerInvariant());    //"just to be safe!"
                                            indexUpdateManager.Update(sourceFilePath.ToLowerInvariant(), xelement);
                                            SwumManager.Instance.AddSourceFile(sourceFilePath.ToLowerInvariant(), xelement);
                                        }
                                        break;
                                }
                                if (args.EventType != FileEventType.FileDeleted)
                                {
                                    lastFile = sourceFilePath.ToLowerInvariant();
                                    lastTime = DateTime.Now;
                                }
                            }
                        }
                    }
                    else
                    {
                        documentIndexer.DeleteDocuments(sourceFilePath, commitImmediately);
                    }
                }
            };
            StartNew(action, cancelTokenSource);
        }



        private static XElement GetXElementForFile(FileEventRaisedArgs args, ISrcMLGlobalService srcMLService)
        {
            XElement xelement = null;
            if (!args.EventType.Equals(FileEventType.FileDeleted))
            {
                if (args.FilePath.EndsWith(".xml") || args.FilePath.EndsWith(".xaml"))
                {
                    var allText = File.ReadAllText(args.FilePath);
                    try
                    {
                        xelement = XDocument.Parse(allText, LoadOptions.SetLineInfo |
                                                        LoadOptions.PreserveWhitespace).Root;
                    }
                    catch (Exception e)
                    {
                        return xelement;
                    }
                }
                else
                    xelement = srcMLService.GetXElementForSourceFile(args.FilePath);
            }
            return xelement;
        }

        private void RemoveTask(Task task, CancellationTokenSource cancelToken)
        {
            lock (tasksTrackerLock)
            {
                tasks.TryTake(out task);
                cancellers.TryTake(out cancelToken);
            }
        }

        private object tasksTrackerLock = new object();
        private string lastFile = "";
        private DateTime lastTime = DateTime.Now;
        

        public void StartupCompleted(object sender, IsReadyChangedEventArgs args)
        {
            if (args.ReadyState)
            {
                if (ServiceLocator.Resolve<SrcMLArchiveEventsHandlers>().TaskCount() == 0)
                {
                    ServiceLocator.Resolve<InitialIndexingWatcher>().InitialIndexingCompleted();
                    SwumManager.Instance.PrintSwumCache();
                }
            }
        }

        public void MonitoringStopped(object sender, EventArgs args)
        {
            lock (tasksTrackerLock)
            {
                foreach (var cancelToken in cancellers)
                    cancelToken.Cancel();
            }

            LogEvents.UIMonitoringStopped(this);
            var currentIndexer = ServiceLocator.ResolveOptional<DocumentIndexer>();
            if (currentIndexer != null)
            {
                currentIndexer.Dispose(false);  // Because in SolutionMonitor: public void StopMonitoring(bool killReaders = false)
            }
            if (SwumManager.Instance != null)
            {
                SwumManager.Instance.PrintSwumCache();
            }
        }

        internal void StartingToIndex()
        {            
            hideProgressBarTimer.Stop();            
            try
            {
                ServiceLocator.Resolve<UIPackage>().UpdateIndexingFilesListIfEmpty();
                ServiceLocator.Resolve<UIPackage>().HandleIndexingStateChange(false);
            }
            catch (Exception ee)
            {
                //ignore
            }                                        
        }

        internal void FinishedIndexing()
        {
            hideProgressBarTimer.Stop();
            hideProgressBarTimer.Start();
        }
       
        void waitToUpdateProgressBar_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                ServiceLocator.Resolve<UIPackage>().HandleIndexingStateChange(true);
            }
            catch (Exception errorToIgnore)
            {
                //ignore
            }
        }






        // Provides a task scheduler that ensures a maximum concurrency level while  
        // running on top of the thread pool. 
        public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
        {
            // Indicates whether the current thread is processing work items.
            [ThreadStatic]
            public static bool _currentThreadIsProcessingItems;

            // The list of tasks to be executed  
            private readonly LinkedList<Task> _tasks = new LinkedList<Task>(); // protected by lock(_tasks) 

            // The maximum concurrency level allowed by this scheduler.  
            private readonly int _maxDegreeOfParallelism;

            // Indicates whether the scheduler is currently processing work items.  
            private int _delegatesQueuedOrRunning = 0;
            private SrcMLArchiveEventsHandlers _handler;

            public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism, SrcMLArchiveEventsHandlers handler)
                : this(maxDegreeOfParallelism)
            {
                _handler = handler;
            }

            // Creates a new instance with the specified degree of parallelism.  
            public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
            {
                if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
                _maxDegreeOfParallelism = maxDegreeOfParallelism;
            }

            public int GetTasks(){

                lock (_tasks)
                    return _tasks.Count();
            }
            

            // Queues a task to the scheduler.  
            protected sealed override void QueueTask(Task task)
            {
                // Add the task to the list of tasks to be processed.  If there aren't enough  
                // delegates currently queued or running to process tasks, schedule another.  
                lock (_tasks)
                {
                    if (_tasks.Count > 2)
                        _handler.StartingToIndex();
                    _tasks.AddLast(task);
                    if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                    {
                        ++_delegatesQueuedOrRunning;
                        NotifyThreadPoolOfPendingWork();
                    }
                }
            }



            // Inform the ThreadPool that there's work to be executed for this scheduler.  
            private void NotifyThreadPoolOfPendingWork()
            {
                ThreadPool.UnsafeQueueUserWorkItem(_ =>
                {
                    // Note that the current thread is now processing work items. 
                    // This is necessary to enable inlining of tasks into this thread.
                    _currentThreadIsProcessingItems = true;
                    try
                    {
                        // Process all available items in the queue. 
                        while (true)
                        {
                            Task item;
                            lock (_tasks)
                            {
                                // When there are no more items to be processed, 
                                // note that we're done processing, and get out. 
                                if (_tasks.Count == 0)
                                {
                                    --_delegatesQueuedOrRunning;
                                    break;
                                }

                                // Get the next item from the queue
                                item = _tasks.First.Value;
                                _tasks.RemoveFirst();
                            }

                            // Execute the task we pulled out of the queue 
                            base.TryExecuteTask(item);
                        }
                    }
                    // We're done processing items on the current thread 
                    finally {
                        _handler.FinishedIndexing();
                        _currentThreadIsProcessingItems = false; 
                    }
                }, null);
            }

            // Attempts to execute the specified task on the current thread.  
            protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                // If this thread isn't already processing a task, we don't support inlining 
                if (!_currentThreadIsProcessingItems) return false;

                // If the task was previously queued, remove it from the queue 
                if (taskWasPreviouslyQueued)
                    // Try to run the task.  
                    if (TryDequeue(task))
                        return base.TryExecuteTask(task);
                    else
                        return false;
                else
                    return base.TryExecuteTask(task);
            }

            // Attempt to remove a previously scheduled task from the scheduler.  
            protected sealed override bool TryDequeue(Task task)
            {
                lock (_tasks) return _tasks.Remove(task);
            }

            // Gets the maximum concurrency level supported by this scheduler.  
            public sealed override int MaximumConcurrencyLevel { get { return _maxDegreeOfParallelism; } }

            // Gets an enumerable of the tasks currently scheduled on this scheduler.  
            protected sealed override IEnumerable<Task> GetScheduledTasks()
            {
                bool lockTaken = false;
                try
                {
                    Monitor.TryEnter(_tasks, ref lockTaken);
                    if (lockTaken) return _tasks;
                    else throw new NotSupportedException();
                }
                finally
                {
                    if (lockTaken) Monitor.Exit(_tasks);
                }
            }

       
        }



    }
}