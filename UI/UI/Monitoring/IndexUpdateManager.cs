using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Sando.Core;
using Sando.Core.Extensions;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer;
using Sando.Indexer.Documents;
using Sando.Indexer.IndexState;

namespace Sando.UI.Monitoring
{
	public class IndexUpdateManager
	{ 

		private FileOperationResolver _fileOperationResolver;
		private IndexFilesStatesManager _indexFilesStatesManager;
		private PhysicalFilesStatesManager _physicalFilesStatesManager;		
		private DocumentIndexer _currentIndexer;


		public IndexUpdateManager(SolutionKey solutionKey, DocumentIndexer currentIndexer, bool isIndexRecreationRequired)
		{
			_currentIndexer = currentIndexer;
			_indexFilesStatesManager = new IndexFilesStatesManager(solutionKey.GetIndexPath(), isIndexRecreationRequired);
			_indexFilesStatesManager.ReadIndexFilesStates();

			_physicalFilesStatesManager = new PhysicalFilesStatesManager();
			_fileOperationResolver = new FileOperationResolver();
		}


		public void SaveFileStates()
		{
			_indexFilesStatesManager.SaveIndexFilesStates();			
		}

		public void UpdateFile(String path)
		{
			try
			{
				

				IndexFileState indexFileState = _indexFilesStatesManager.GetIndexFileState(path);
				PhysicalFileState physicalFileState = _physicalFilesStatesManager.GetPhysicalFileState(path);
				IndexOperation requiredIndexOperation = _fileOperationResolver.ResolveRequiredOperation(physicalFileState, indexFileState);

				switch(requiredIndexOperation)
				{
					case IndexOperation.Add:
						{
                            _currentIndexer.DeleteDocuments(path); //just to be safe!
							Update(indexFileState, path, physicalFileState);
							break;
						}
						;
					case IndexOperation.Update:
						{
							_currentIndexer.DeleteDocuments(path);
							Update(indexFileState, path, physicalFileState);
							break;
						}
						;
					case IndexOperation.DoNothing:
						{
							break;
						}
				}
			}
			catch(ArgumentException argumentException)
			{
				//ignore items with no associated file
			}
			catch(XmlException xmlException)
			{
				//TODO - should fix this if it happens too often
				//TODO - need to investigate why this is happening during parsing
				Debug.WriteLine(xmlException);
			}
			catch(NullReferenceException nre)
			{
				//TODO - these need to be handled
				//TODO - need to investigate why this is happening during parsing
				Debug.WriteLine(nre);
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


	}
}
