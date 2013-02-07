using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
// Code changed by JZ: solution monitor integration
using System.Xml.Linq;
// End of code changes
using Sando.Core;
using Sando.Core.Extensions;
using Sando.Core.Extensions.Logging;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.IndexerContracts;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer;
using Sando.Indexer.Documents;
using Sando.Indexer.IndexState;
using Sando.Recommender;

namespace Sando.UI.Monitoring
{
    public class IndexUpdateManager
	{
        private IIndexFilterManager _indexFilterManager;
		private FileOperationResolver _fileOperationResolver;
		private IndexFilesStatesManager _indexFilesStatesManager;
		private PhysicalFilesStatesManager _physicalFilesStatesManager;		
		private DocumentIndexer _currentIndexer;


		public IndexUpdateManager(DocumentIndexer currentIndexer, bool isIndexRecreationRequired)
		{
		    var solutionKey = ServiceLocator.Resolve<SolutionKey>();
			_currentIndexer = currentIndexer;

            _indexFilterManager = ExtensionPointsRepository.Instance.GetIndexFilterManagerImplementation();

			_indexFilesStatesManager = new IndexFilesStatesManager(solutionKey.IndexPath, isIndexRecreationRequired);
			_indexFilesStatesManager.ReadIndexFilesStates();

			_physicalFilesStatesManager = new PhysicalFilesStatesManager();
			_fileOperationResolver = new FileOperationResolver();
		}

        // Code added by JZ: solution monitor integration
        /// <summary>
        /// New constructor.
        /// The another (original) constructor should never be used any more.
        /// </summary>
        /// <param name="currentIndexer"></param>
        public IndexUpdateManager(DocumentIndexer currentIndexer)
        {
            _currentIndexer = currentIndexer;
        }
        // End of code changes

		public void SaveFileStates()
		{
			_indexFilesStatesManager.SaveIndexFilesStates();			
		}

        // Code added by JZ: To complete the Delete case (obsolete because it is called only in Sando's solution monitor)
        public List<string> GetAllIndexedFileNames()
        {
            return _indexFilesStatesManager.GetAllIndexedFileNames();
        }
        // End of code changes

		public void UpdateFile(String path)
		{
			try
			{
                IndexOperation requiredIndexOperation;
			    IndexFileState indexFileState = null;
			    PhysicalFileState physicalFileState = null;
			    if (!_indexFilterManager.ShouldFileBeIndexed(path))
			    {
			        requiredIndexOperation = IndexOperation.Delete;
			    }
			    else
			    {
			        indexFileState = _indexFilesStatesManager.GetIndexFileState(path);
			        physicalFileState = _physicalFilesStatesManager.GetPhysicalFileState(path);
			        requiredIndexOperation = _fileOperationResolver.ResolveRequiredOperation(physicalFileState, indexFileState);
			    }

			    switch(requiredIndexOperation)
				{
					case IndexOperation.Add:
						{
							Debug.WriteLine("IndexOperation.Add");
							_currentIndexer.DeleteDocuments(path); //just to be safe!
							Update(indexFileState, path, physicalFileState);
                            //compute SWUM on the file, for the query recommender
                            SwumManager.Instance.AddSourceFile(path);
							break;
						}
						;
					case IndexOperation.Update:
						{
							Debug.WriteLine("IndexOperation.Update");
							_currentIndexer.DeleteDocuments(path);
							Update(indexFileState, path, physicalFileState);
						    SwumManager.Instance.UpdateSourceFile(path); //for the query recommender
							break;
						}
						;
                    // Code changed by JZ: Added the Delete case (obsolete because UpdateFile() is called only in Sando's solution monitor)
                    case IndexOperation.Delete:
                        {
                            _currentIndexer.DeleteDocuments(path);
                            break;
                        }
                        ;
                    // End of code changes
                    case IndexOperation.DoNothing:
						{
							Debug.WriteLine("IndexOperation.DoNothing");
							break;
						}
				}
			}
			catch(ArgumentException argumentException)
			{
				//ignore items with no associated file
                FileLogger.DefaultLogger.Error(ExceptionFormatter.CreateMessage(argumentException));
			}
			catch(XmlException xmlException)
			{
				//TODO - should fix this if it happens too often
				//TODO - need to investigate why this is happening during parsing
                FileLogger.DefaultLogger.Error(ExceptionFormatter.CreateMessage(xmlException));
			}
			catch(NullReferenceException nre)
			{
				//TODO - these need to be handled
				//TODO - need to investigate why this is happening during parsing
                FileLogger.DefaultLogger.Error(ExceptionFormatter.CreateMessage(nre));
			}

		}

		private void Update(IndexFileState indexFileState, string filePath, PhysicalFileState physicalFileState)
		{
			DateTime? lastModificationDate = physicalFileState.LastModificationDate;
			if(indexFileState == null)
				indexFileState = new IndexFileState(filePath, lastModificationDate);
			else
				indexFileState.LastIndexingDate = lastModificationDate;
			FileInfo fileInfo = new FileInfo(filePath);
			var parsed = ExtensionPointsRepository.Instance.GetParserImplementation(fileInfo.Extension).Parse(filePath);

            var unresolvedElements = parsed.FindAll(pe => pe is CppUnresolvedMethodElement);
            if (unresolvedElements.Count > 0)
            {
				//first generate program elements for all the included headers
                List<ProgramElement> headerElements = CppHeaderElementResolver.GenerateCppHeaderElements(filePath, unresolvedElements);

                //then try to resolve
                foreach (CppUnresolvedMethodElement unresolvedElement in unresolvedElements)
                {
                    SandoDocument document = CppHeaderElementResolver.GetDocumentForUnresolvedCppMethod(unresolvedElement, headerElements);
                    if (document != null)
                        _currentIndexer.AddDocument(document);
                }
            }

			foreach(var programElement in parsed)
			{
				if(! (programElement is CppUnresolvedMethodElement))
                {
					var document = DocumentFactory.Create(programElement);
                    if(document!=null)
					    _currentIndexer.AddDocument(document);
				}
			}
			_indexFilesStatesManager.UpdateIndexFileState(filePath, indexFileState);
		}

        // Code added by JZ: solution monitor integration
        /// <summary>
        /// New Update() method that takes both source file path and the XElement representation of the source file as input arguments.
        /// TODO: what if the variable parsed is null?
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="xelement"></param>
        public void Update(string filePath, XElement xElement)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            writeLog( "IndexUpdateManager.Update(): " + filePath + " [" + fileInfo.Extension + "]");
            try
            {
                var parsed = ExtensionPointsRepository.Instance.GetParserImplementation(fileInfo.Extension).Parse(filePath, xElement);
                ////var parsed = ExtensionPointsRepository.Instance.GetParserImplementation(fileInfo.Extension).Parse(filePath);

                var unresolvedElements = parsed.FindAll(pe => pe is CppUnresolvedMethodElement);
                if (unresolvedElements.Count > 0)
                {
                    //first generate program elements for all the included headers
                    List<ProgramElement> headerElements = CppHeaderElementResolver.GenerateCppHeaderElements(filePath, unresolvedElements);

                    //then try to resolve
                    foreach (CppUnresolvedMethodElement unresolvedElement in unresolvedElements)
                    {
                        SandoDocument document = CppHeaderElementResolver.GetDocumentForUnresolvedCppMethod(unresolvedElement, headerElements);
                        if (document != null)
                        {
                            //writeLog( "- DI.AddDocument()");
                            _currentIndexer.AddDocument(document);
                        }
                    }
                }

                foreach (var programElement in parsed)
                {
                    if (!(programElement is CppUnresolvedMethodElement))
                    {
                        var document = DocumentFactory.Create(programElement);
                        if (document != null)
                        {
                            //writeLog( "- DI.AddDocument()");
                            _currentIndexer.AddDocument(document);
                        }
                    }
                }
                }
            catch (Exception e)
            {
                writeLog( "Exception in IndexUpdateManager.Update() " + e.Message + "\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// For debugging.
        /// </summary>
        /// <param name="logFile"></param>
        /// <param name="str"></param>
        private static void writeLog(string str)
        {
            FileLogger.DefaultLogger.Info(str);
        }
        // End of code changes
	}
}
