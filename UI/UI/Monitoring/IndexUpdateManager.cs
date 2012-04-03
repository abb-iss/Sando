using System;
using System.Diagnostics;
using System.Xml;
using Sando.Core;
using Sando.Indexer;
using Sando.Indexer.Documents;
using Sando.Indexer.IndexState;
using Sando.Parser;

namespace Sando.UI.Monitoring
{
	public class IndexUpdateManager
	{ 

		private FileOperationResolver _fileOperationResolver;
		private IndexFilesStatesManager _indexFilesStatesManager;
		private PhysicalFilesStatesManager _physicalFilesStatesManager;		
		private DocumentIndexer _currentIndexer;
	    private ParserManager _parserManager;


	    public IndexUpdateManager(SolutionKey solutionKey, DocumentIndexer currentIndexer)
		{
			_currentIndexer = currentIndexer;
			_indexFilesStatesManager = new IndexFilesStatesManager(solutionKey.GetIndexPath());
			_indexFilesStatesManager.ReadIndexFilesStates();

			_physicalFilesStatesManager = new PhysicalFilesStatesManager();
			_fileOperationResolver = new FileOperationResolver();
	        _parserManager = new ParserManager();
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

            var parsed = _parserManager.Get(filePath).Parse(filePath);
			foreach(var programElement in parsed)
			{
				if(programElement is CppUnresolvedMethodElement)
				{
					CppUnresolvedMethodElement unresolvedMethod = (CppUnresolvedMethodElement)programElement;
					foreach(String headerFile in unresolvedMethod.IncludeFileNames)
					{
						bool isResolved = false;
						MethodElement methodElement = null;

						//it's reasonable to assume that the header file path is relative from the cpp file,
						//as other included files are unlikely to be part of the same project and therefore 
						//should not need to be parsed
						string headerPath = System.IO.Path.GetDirectoryName(filePath) + "\\" + headerFile;
						if(!System.IO.File.Exists(headerPath)) continue;

						isResolved = unresolvedMethod.TryResolve(_parserManager.Get(headerPath).Parse(headerPath), out methodElement);
						if(isResolved == true)
						{
							var document = DocumentFactory.Create(methodElement);
							_currentIndexer.AddDocument(document);
							break;
						}
					}
				}
				else
				{
					var document = DocumentFactory.Create(programElement);
					_currentIndexer.AddDocument(document);
				}
			}
			_indexFilesStatesManager.UpdateIndexFileState(filePath, indexFileState);
		}

        class ParserManager
        {

            private readonly ParserInterface _csParser = new SrcMLParser(new SrcMLGenerator(LanguageEnum.CSharp));
            private readonly ParserInterface _cppParser = new SrcMLParser(new SrcMLGenerator(LanguageEnum.CPP));

            internal ParserManager()
            {
                _csParser = new SrcMLParser(SrcMLGenerator.Generator(LanguageEnum.CSharp).SetSrcMLLocation(SrcMLParser.StandardSrcMlLocation));
                _cppParser = new SrcMLParser(SrcMLGenerator.Generator(LanguageEnum.CPP).SetSrcMLLocation(SrcMLParser.StandardSrcMlLocation));                                
            }

            internal ParserInterface Get(String filePath)
            {
                if (filePath.EndsWith(".cs"))
                {
                    return _csParser;
                }
                else
                {
                    return _cppParser;
                }
            }
        }


	}
}
