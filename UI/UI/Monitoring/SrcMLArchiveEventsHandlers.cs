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
        // JZ: SrcMLService Integration
        /// <summary>
        /// Respond to the SourceFileChanged event from SrcMLService.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void SourceFileChanged(object sender, FileEventRaisedArgs args)
        {
            FileLogger.DefaultLogger.Info("Sando: SourceFileChanged(), File = " + args.FilePath + ", OldFile = " + args.OldFilePath + ", EventType = " + args.EventType + ", HasSrcML = " + args.HasSrcML);

            // Ignore files that can not be indexed by Sando.
            var fileExtension = Path.GetExtension(args.FilePath);
            if (fileExtension != null && !fileExtension.Equals(String.Empty))
            {
                string sourceFilePath = args.FilePath;
                string oldSourceFilePath = args.OldFilePath;
                var documentIndexer = ServiceLocator.Resolve<DocumentIndexer>();
                if(ServiceLocator.Resolve<IndexFilterManager>().ShouldFileBeIndexed(args.FilePath))
                {
                    if (ExtensionPointsRepository.Instance.GetParserImplementation(fileExtension) != null)
                    {
                        var service = (sender as ABB.SrcML.VisualStudio.SrcMLService.ISrcMLGlobalService);
                        var xelement = service.GetXElementForSourceFile(args.FilePath);

                        var indexUpdateManager = ServiceLocator.Resolve<IndexUpdateManager>();

                        switch (args.EventType)
                        {
                            case FileEventType.FileAdded:
                                documentIndexer.DeleteDocuments(sourceFilePath);    //"just to be safe!"
                                indexUpdateManager.Update(sourceFilePath, xelement);
                                SwumManager.Instance.AddSourceFile(sourceFilePath);
                                break;
                            case FileEventType.FileChanged:
                                documentIndexer.DeleteDocuments(sourceFilePath);
                                indexUpdateManager.Update(sourceFilePath, xelement);
                                SwumManager.Instance.UpdateSourceFile(sourceFilePath);
                                break;
                            case FileEventType.FileDeleted:
                                documentIndexer.DeleteDocuments(sourceFilePath);
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
                    documentIndexer.DeleteDocuments(sourceFilePath);
                }
            }
        }

        /// <summary>
        /// Respond to the StartupCompleted event from SrcMLService.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void StartupCompleted(object sender, EventArgs args)
        {
            FileLogger.DefaultLogger.Info("Sando: StartupCompleted()");

            ServiceLocator.Resolve<InitialIndexingWatcher>().InitialIndexingCompleted();
            SwumManager.Instance.PrintSwumCache();
        }

        /// <summary>
        /// Respond to the MonitoringStopped event from SrcMLService.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void MonitoringStopped(object sender, EventArgs args)
        {
            FileLogger.DefaultLogger.Info("Sando: MonitoringStopped()");

            var currentIndexer = ServiceLocator.ResolveOptional<DocumentIndexer>();
            if (currentIndexer != null)
            {
                currentIndexer.Dispose(false);  // Because in original SolutionMonitor: public void StopMonitoring(bool killReaders = false)
            }
            if (SwumManager.Instance != null)
            {
                SwumManager.Instance.PrintSwumCache();
            }
        }
        // End of code changes
    }
}