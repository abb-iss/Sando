using System;
using System.IO;
using ABB.SrcML;
using ABB.SrcML.VisualStudio.SrcMLService;
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
            FileLogger.DefaultLogger.Info("Sando: SourceFileChanged(), File = " + args.FilePath + ", OldFile = " + args.OldFilePath + ", EventType = " + args.EventType + ", HasSrcML = " + args.HasSrcML);
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
                        var xelement = srcMLService.GetXElementForSourceFile(args.FilePath);
                        //var xelement = args.SrcMLXElement;

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
                                documentIndexer.DeleteDocuments(sourceFilePath,commitImmediately);
                                SwumManager.Instance.RemoveSourceFile(sourceFilePath);
                                break;
                            case FileEventType.FileRenamed: // FileRenamed is actually never raised.
                                documentIndexer.DeleteDocuments(oldSourceFilePath);
                                indexUpdateManager.Update(sourceFilePath, xelement);
                                SwumManager.Instance.UpdateSourceFile(sourceFilePath, xelement);
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