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
using ABB.SrcML.Utilities;


namespace Sando.UI.Monitoring
{
    public class SrcMLArchiveEventsHandlers
    {

        private ConcurrentBag<Task> tasks = new ConcurrentBag<Task>();

        public void SourceFileChanged(object sender, FileEventRaisedArgs args)
        {
            SourceFileChanged(sender, args, false);
        }

        public void WaitForIndexing()
        {
            bool running = true;
            while (running)
            {
                Thread.Sleep(500);
                lock (tasksTrackerLock)
                {
                    running = tasks.Count > 0;
                }
            }            
        }

        public void SourceFileChanged(object sender, FileEventRaisedArgs args, bool commitImmediately = false)
        {

            var task = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
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
                            
                            var xelement = GetXElementForFile(args, srcMLService);


                            var indexUpdateManager = ServiceLocator.Resolve<IndexUpdateManager>();

                            switch (args.EventType)
                            {
                                case FileEventType.FileAdded:
                                    documentIndexer.DeleteDocuments(sourceFilePath);    //"just to be safe!"
                                    indexUpdateManager.Update(sourceFilePath, xelement);
                                    SwumManager.Instance.AddSourceFile(sourceFilePath, xelement);
                                    break;
                                case FileEventType.FileChanged:
                                    documentIndexer.DeleteDocuments(sourceFilePath);
                                    indexUpdateManager.Update(sourceFilePath, xelement);
                                    SwumManager.Instance.UpdateSourceFile(sourceFilePath, xelement);
                                    break;
                                case FileEventType.FileDeleted:
                                    documentIndexer.DeleteDocuments(sourceFilePath, commitImmediately);
                                    SwumManager.Instance.RemoveSourceFile(sourceFilePath);
                                    break;
                                case FileEventType.FileRenamed: // FileRenamed is actually never raised.
                                    throw new NotImplementedException();
                                    //documentIndexer.DeleteDocuments(oldSourceFilePath);
                                    //indexUpdateManager.Update(sourceFilePath, xelement);
                                    //SwumManager.Instance.UpdateSourceFile(sourceFilePath, xelement);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        documentIndexer.DeleteDocuments(sourceFilePath, commitImmediately);
                    }
                }
            });
            lock (tasksTrackerLock)
            {
                tasks.Add(task);
            }
            task.ContinueWith(removeTask => RemoveTask(task));
        }

        private static XElement GetXElementForFile(FileEventRaisedArgs args, ISrcMLGlobalService srcMLService)
        {
            XElement xelement = null;
            if (!args.EventType.Equals(FileEventType.FileDeleted))
            {
                if (args.FilePath.EndsWith(".xml") || args.FilePath.EndsWith(".xaml"))
                {
                    var allText = File.ReadAllText(args.FilePath);
                    xelement = XDocument.Parse(allText, LoadOptions.SetLineInfo |
                                                        LoadOptions.PreserveWhitespace).Root;
                }
                else
                    xelement = srcMLService.GetXElementForSourceFile(args.FilePath);
            }
            return xelement;
        }

        private void RemoveTask(Task task)
        {
            lock (tasksTrackerLock)
                tasks.TryTake(out task);            
        }

        private object tasksTrackerLock = new object();

        public void StartupCompleted(object sender, IsReadyChangedEventArgs args)
        {
            if (args.ReadyState)
            {
                ServiceLocator.Resolve<SrcMLArchiveEventsHandlers>().WaitForIndexing();
                ServiceLocator.Resolve<InitialIndexingWatcher>().InitialIndexingCompleted();
                SwumManager.Instance.PrintSwumCache();
            }
        }

        public void MonitoringStopped(object sender, EventArgs args)
        {
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

        
    }
}