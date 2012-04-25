using System;
using System.Diagnostics;
using System.Xml;
using Sando.Core;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer;
using Sando.Indexer.Documents;
using Sando.Indexer.IndexState;
using Sando.Parser;
using Sando.Core.Extensions;
using System.IO;
using System.Collections.Generic;

namespace Sando.UI.Monitoring
{
	public class IndexUpdateManager
	{ 

		private FileOperationResolver _fileOperationResolver;
		private IndexFilesStatesManager _indexFilesStatesManager;
		private PhysicalFilesStatesManager _physicalFilesStatesManager;		
		private DocumentIndexer _currentIndexer;


	    public IndexUpdateManager(SolutionKey solutionKey, DocumentIndexer currentIndexer)
		{
			_currentIndexer = currentIndexer;
			_indexFilesStatesManager = new IndexFilesStatesManager(solutionKey.GetIndexPath());
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
                List<ProgramElement> headerElements = GenerateCppHeaderElements(filePath, unresolvedElements);

                //now try to resolve
                foreach (CppUnresolvedMethodElement unresolvedElement in unresolvedElements)
                {
                    SandoDocument document = GetDocumentForUnresolvedCppMethod(unresolvedElement, headerElements);
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

        private List<ProgramElement> GenerateCppHeaderElements(string filePath, List<ProgramElement> unresolvedElements)
        {
            List<ProgramElement> headerElements = new List<ProgramElement>();

            //first parse all the included header files. they are the same in all the unresolved elements
            CppUnresolvedMethodElement firstUnresolved = (CppUnresolvedMethodElement)unresolvedElements[0];
            foreach (String headerFile in firstUnresolved.IncludeFileNames)
            {
                //it's reasonable to assume that the header file path is relative from the cpp file,
                //as other included files are unlikely to be part of the same project and therefore 
                //should not need to be parsed
                string headerPath = System.IO.Path.GetDirectoryName(filePath) + "\\" + headerFile;
                if (!System.IO.File.Exists(headerPath)) continue;
                Debug.WriteLine("*** parsing header = " + headerPath);
                var headerInfo = new FileInfo(headerPath);
                headerElements.AddRange(ExtensionPointsRepository.Instance.GetParserImplementation(headerInfo.Extension).Parse(headerPath));
            }
            return headerElements;
        }

        //TODO - it seems wrong that we have language-specific code in the indexmanager
        private SandoDocument GetDocumentForUnresolvedCppMethod(CppUnresolvedMethodElement unresolvedMethod, List<ProgramElement> headerElements)
	    {
            bool isResolved = false;
            MethodElement methodElement = null;

            isResolved = unresolvedMethod.TryResolve(headerElements, out methodElement);
            if (isResolved == true)
            {
                return DocumentFactory.Create(methodElement);
            }
            return null;
	    }
	}
}
