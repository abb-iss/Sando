using System;
using System.IO;
using ABB.SrcML;
using Sando.Core.Extensions;
using Sando.Core.Extensions.Logging;
using Sando.DependencyInjection;
using Sando.Indexer.IndexFiltering;
using Sando.Recommender;

namespace Sando.UI.Monitoring
{
    public class SrcMLArchiveEventsHandlers
    {
        public void SourceFileChanged(object sender, FileEventRaisedArgs args)
        {
            FileLogger.DefaultLogger.Info("Sando: RespondToSourceFileChangedEvent(), File = " + args.SourceFilePath + ", EventType = " + args.EventType);            
            // Ignore files that can not be indexed by Sando.
		    var fileExtension = Path.GetExtension(args.SourceFilePath);
            if (fileExtension != null && !fileExtension.Equals(String.Empty))
            {
                string sourceFilePath = args.SourceFilePath;
                string oldSourceFilePath = args.OldSourceFilePath;
                if (ServiceLocator.Resolve<IndexFilterManager>().ShouldFileBeIndexed(args.SourceFilePath))
                {
                    if (ExtensionPointsRepository.Instance.GetParserImplementation(fileExtension) != null)
                    {
                        var xelement = args.SrcMLXElement;

                        switch (args.EventType)
                        {
                            case FileEventType.FileAdded:
                                SolutionMonitorFactory.DeleteIndex(sourceFilePath);
                                    //"just to be safe!" from IndexUpdateManager.UpdateFile()
                                SolutionMonitorFactory.UpdateIndex(sourceFilePath, xelement);
                                SwumManager.Instance.AddSourceFile(sourceFilePath);
                                break;
                            case FileEventType.FileChanged:
                                SolutionMonitorFactory.DeleteIndex(sourceFilePath);
                                SolutionMonitorFactory.UpdateIndex(sourceFilePath, xelement);
                                SwumManager.Instance.UpdateSourceFile(sourceFilePath);
                                break;
                            case FileEventType.FileDeleted:
                                SolutionMonitorFactory.DeleteIndex(sourceFilePath);
                                SwumManager.Instance.RemoveSourceFile(sourceFilePath);
                                break;
                            case FileEventType.FileRenamed: // FileRenamed is actually never raised.
                                SolutionMonitorFactory.DeleteIndex(oldSourceFilePath);
                                SolutionMonitorFactory.UpdateIndex(sourceFilePath, xelement);
                                SwumManager.Instance.UpdateSourceFile(sourceFilePath);
                                break;
                        }
                    }
                }
                else
                {
                    SolutionMonitorFactory.DeleteIndex(sourceFilePath);
                }
            }
        }

        public void StartupCompleted(object sender, EventArgs args)
        {
            SolutionMonitorFactory.StartupCompleted();
            SwumManager.Instance.PrintSwumCache();
        }

        public void MonitoringStopped(object sender, EventArgs args)
        {
            SolutionMonitorFactory.MonitoringStopped();
            if (SwumManager.Instance != null)
            {
                SwumManager.Instance.PrintSwumCache();
            }
        }
    }
}