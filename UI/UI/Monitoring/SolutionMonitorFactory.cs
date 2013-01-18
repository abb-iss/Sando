using System;
using System.Diagnostics.Contracts;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Sando.Core;
using Sando.Indexer;
// Code changed by JZ: solution monitor integration
using System.Xml.Linq;
//using ABB.SrcML.VisualStudio.SolutionMonitor;
// End of code changes

namespace Sando.UI.Monitoring
{
	class SolutionMonitorFactory
	{
        private static DocumentIndexer currentIndexer;

		private const string Lucene = "\\lucene";

	    public static string LuceneDirectory { get; set; }


        // Code changed by JZ: solution monitor integration
        // These variables are moved from Sando's SolutionMonitor
        public static SolutionKey _solutionKey { get; set; }
        public static string _currentPath { get; set; }
        public static bool _initialIndexDone { get; set; }
        
        /// <summary>
        /// Constructor.
        /// Use SrcML.NET's SolutionMonior, instead of Sando's SolutionMonitor
        /// </summary>
        /// <param name="isIndexRecreationRequired"></param>
        /// <returns></returns>
        public static ABB.SrcML.VisualStudio.SolutionMonitor.SolutionMonitor CreateMonitor(bool isIndexRecreationRequired)
		{
			var openSolution = UIPackage.GetOpenSolution();
			return CreateMonitor(openSolution, isIndexRecreationRequired);
		}

        /// <summary>
        /// Constructor.
        /// Use SrcML.NET's SolutionMonior, instead of Sando's SolutionMonitor
        /// </summary>
        /// <param name="openSolution"></param>
        /// <param name="isIndexRecreationRequired"></param>
        /// <returns></returns>
        private static ABB.SrcML.VisualStudio.SolutionMonitor.SolutionMonitor CreateMonitor(Solution openSolution, bool isIndexRecreationRequired)
		{
			Contract.Requires(openSolution != null, "A solution must be open");

			//TODO if solution is reopen - the guid should be read from file - future change
			SolutionKey sk = new SolutionKey(Guid.NewGuid(), openSolution.FileName, GetLuceneDirectoryForSolution(openSolution));
            currentIndexer = DocumentIndexerFactory.CreateIndexer(sk, AnalyzerType.Snowball);
			if(isIndexRecreationRequired)
			{
				currentIndexer.DeleteDocuments("*");
				currentIndexer.CommitChanges();
			}

            // Create a new instance of SrcML.NET's solution monitor
            var currentMonitor = new ABB.SrcML.VisualStudio.SolutionMonitor.SolutionMonitor(ABB.SrcML.VisualStudio.SolutionMonitor.SolutionWrapper.Create(openSolution));
            // Subscribe events from SrcML.NET's solution monitor
            //currentMonitor.SrcMLDOTNETEventRaised += RespondToSrcMLDOTNETEvent;

            // These variables are moved from Sando's SolutionMonitor
            _solutionKey = sk;
            _currentPath = sk.GetIndexPath();
            _initialIndexDone = false;

			return currentMonitor;
		}
        
        /// <summary>
        /// Respond to the events from SrcML.NET.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public static void RespondToSrcMLDOTNETEvent(object sender, ABB.SrcML.SrcMLDOTNETEventArgs eventArgs)
        {
            writeLog("D:\\Data\\log.txt", "SMF's RespondToSrcMLDOTNETEvent(), eventArgs.EventType = " + eventArgs.EventType);
            string sourceFilePath = eventArgs.SourceFilePath;
            XElement xelement = eventArgs.SrcMLXElement;
            switch (eventArgs.EventType)
            {
                case ABB.SrcML.SrcMLDOTNETEventType.SourceFileAdded:
                    writeLog("D:\\Data\\log.txt", "!! TO ADD index: " + sourceFilePath);
                    currentIndexer.DeleteDocuments(sourceFilePath); //"just to be safe!" from IndexUpdateManager.UpdateFile()
                    UpdateIndex(sourceFilePath, xelement);
                    currentIndexer.CommitChanges();
                    break;
                case ABB.SrcML.SrcMLDOTNETEventType.SourceFileChanged:
                    writeLog("D:\\Data\\log.txt", "!! TO CHANGE index: " + sourceFilePath);
                    currentIndexer.DeleteDocuments(sourceFilePath);
                    UpdateIndex(sourceFilePath, xelement);
                    currentIndexer.CommitChanges();
                    break;
                case ABB.SrcML.SrcMLDOTNETEventType.SourceFileDeleted:
                    writeLog("D:\\Data\\log.txt", "!! TO DELETE index: " + sourceFilePath);
                    currentIndexer.DeleteDocuments(sourceFilePath);
                    currentIndexer.CommitChanges();
                    break;
                case ABB.SrcML.SrcMLDOTNETEventType.StartupCompleted:
                    writeLog("D:\\Data\\log.txt", "!! Initial indexing DONE");
                    _initialIndexDone = true;
                    currentIndexer.CommitChanges(); // TODO: Maybe removed because already committed during Add/Change/Delete above
                    break;
                case ABB.SrcML.SrcMLDOTNETEventType.MonitoringStopped:
                    writeLog("D:\\Data\\log.txt", "!! TO SHUT DOWN the current indexer");
                    if (currentIndexer != null)
                    {
                        currentIndexer.CommitChanges();
                        ////_indexUpdateManager.SaveFileStates();
                        currentIndexer.Dispose(false);  // Because in SolutionMonitor: public void StopMonitoring(bool killReaders = false)
                        currentIndexer = null;
                    }
                    break;
            }
        }

        /// <summary>
        /// Update index.
        /// </summary>
        /// <param name="filePath">Source file path</param>
        /// <param name="xelement">XElement of the source file, generated by SrcML.NET</param>
        private static void UpdateIndex(string filePath, XElement xelement)
        {
            IndexUpdateManager indexUpdateManager = new IndexUpdateManager(currentIndexer);
            indexUpdateManager.Update(filePath, xelement);
        }

        // Moved from SolutionMonitor.cs, don't know if it is still useful
        public static void AddUpdateListener(IIndexUpdateListener listener)
        {
            currentIndexer.AddIndexUpdateListener(listener);
        }

        // Moved from SolutionMonitor.cs, don't know if it is still useful
        public static void RemoveUpdateListener(IIndexUpdateListener listener)
        {
            currentIndexer.RemoveIndexUpdateListener(listener);
        }

        /* //// Original implementation
        public static SolutionMonitor CreateMonitor(bool isIndexRecreationRequired)
        {
            var openSolution = UIPackage.GetOpenSolution();
            return CreateMonitor(openSolution, isIndexRecreationRequired);
        }

        private static SolutionMonitor CreateMonitor(Solution openSolution, bool isIndexRecreationRequired)
        {
            Contract.Requires(openSolution != null, "A solution must be open");

            //TODO if solution is reopen - the guid should be read from file - future change
            SolutionKey solutionKey = new SolutionKey(Guid.NewGuid(), openSolution.FileName, GetLuceneDirectoryForSolution(openSolution));
            var currentIndexer = DocumentIndexerFactory.CreateIndexer(solutionKey, AnalyzerType.Snowball);
            if (isIndexRecreationRequired)
            {
                currentIndexer.DeleteDocuments("*");
                currentIndexer.CommitChanges();
            }
            var currentMonitor = new SolutionMonitor(SolutionWrapper.Create(openSolution), solutionKey, currentIndexer, isIndexRecreationRequired);
            return currentMonitor;
        }
        */
        // End of code changes

        private static string CreateLuceneFolder()
		{
            Contract.Requires(LuceneDirectory != null, "Please set the LuceneDirectory before calling this method");
			return CreateFolder(Lucene, LuceneDirectory);
		}

		private static string CreateFolder(string folderName, string parentDirectory)
		{
			if (!File.Exists(parentDirectory + folderName))
			{
				var directoryInfo = Directory.CreateDirectory(parentDirectory + folderName);
				return directoryInfo.FullName;
			}
			else
			{
                return parentDirectory + folderName;
			}
		}

		private static string GetName(Solution openSolution)
		{
			var fullName = openSolution.FullName;
			var split = fullName.Split('\\');
			return split[split.Length - 1]+fullName.GetHashCode();
		}



		private static string GetLuceneDirectoryForSolution(Solution openSolution)
		{
		    var luceneFolder = CreateLuceneFolder();
            CreateFolder(GetName(openSolution), luceneFolder + "\\");
			return luceneFolder + "\\" + GetName(openSolution);
		}





        // Code changed by JZ on 1/10: solution monitor integration
        /// <summary>
        /// For debugging.
        /// </summary>
        /// <param name="logFile"></param>
        /// <param name="str"></param>
        private static void writeLog(string logFile, string str)
        {
            StreamWriter sw = new StreamWriter(logFile, true, System.Text.Encoding.ASCII);
            sw.WriteLine(str);
            sw.Close();
        }
        // End of code changes
    }
}