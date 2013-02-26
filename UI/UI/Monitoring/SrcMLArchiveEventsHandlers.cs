using System;
using System.IO;
using ABB.SrcML;
using Sando.Core.Extensions;
using Sando.Core.Extensions.Logging;
using Sando.DependencyInjection;
using Sando.Indexer;
using Sando.Indexer.IndexFiltering;
using Sando.Recommender;

namespace Sando.UI.Monitoring
{
    public class SrcMLArchiveEventsHandlers
    {
        public void SourceFileChanged(object sender, FileEventRaisedArgs args)
        {
            SourceFileChanged(sender, args, false);
        }

        public void SourceFileChanged(object sender, FileEventRaisedArgs args, bool commitImmediately = false)
        {
            FileLogger.DefaultLogger.Info("Sando: RespondToSourceFileChangedEvent(), File = " + args.SourceFilePath + ", EventType = " + args.EventType);            
            // Ignore files that can not be indexed by Sando.
		    var fileExtension = Path.GetExtension(args.SourceFilePath);
            if (fileExtension != null && !fileExtension.Equals(String.Empty))
            {
                string sourceFilePath = args.SourceFilePath;
                string oldSourceFilePath = args.OldSourceFilePath;
                var documentIndexer = ServiceLocator.Resolve<DocumentIndexer>();
                if (ServiceLocator.Resolve<IndexFilterManager>().ShouldFileBeIndexed(args.SourceFilePath))
                {
                    if (ExtensionPointsRepository.Instance.GetParserImplementation(fileExtension) != null)
                    {
                        var xelement = args.SrcMLXElement;
                        var indexUpdateManager = ServiceLocator.Resolve<IndexUpdateManager>();

                        switch (args.EventType)
                        {
                            case FileEventType.FileAdded:
                                documentIndexer.DeleteDocuments(sourceFilePath);
                                    //"just to be safe!" from IndexUpdateManager.UpdateFile()
                                indexUpdateManager.Update(sourceFilePath, xelement);
                                SwumManager.Instance.AddSourceFile(sourceFilePath);
                                break;
                            case FileEventType.FileChanged:
                                documentIndexer.DeleteDocuments(sourceFilePath);
                                indexUpdateManager.Update(sourceFilePath, xelement);
                                SwumManager.Instance.UpdateSourceFile(sourceFilePath);
                                break;
                            case FileEventType.FileDeleted:
                                documentIndexer.DeleteDocuments(sourceFilePath,commitImmediately);
                                SwumManager.Instance.RemoveSourceFile(sourceFilePath);
                                break;
                            case FileEventType.FileRenamed: // FileRenamed is actually never raised.
                                documentIndexer.DeleteDocuments(oldSourceFilePath);
                                indexUpdateManager.Update(sourceFilePath, xelement);
                                SwumManager.Instance.UpdateSourceFile(sourceFilePath);
                                break;
                        }
                    }
                }
                else
                {
                    documentIndexer.DeleteDocuments(sourceFilePath,commitImmediately);
                }
            }
        }

        public void StartupCompleted(object sender, EventArgs args)
        {
            ServiceLocator.Resolve<InitialIndexingWatcher>().InitialIndexingCompleted();
            SwumManager.Instance.PrintSwumCache();
        }

        public void MonitoringStopped(object sender, EventArgs args)
        {
            FileLogger.DefaultLogger.Info("Sando: MonitoringStopped()");
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