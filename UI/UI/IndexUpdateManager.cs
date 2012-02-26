using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using EnvDTE;
using Sando.Core;
using Sando.Indexer;
using Sando.Indexer.Documents;
using Sando.Indexer.IndexState;
using Sando.Parser;

namespace Sando.UI
{
	class IndexUpdateManager
	{
		private FileOperationResolver _fileOperationResolver;
		private IndexFilesStatesManager _indexFilesStatesManager;
		private PhysicalFilesStatesManager _physicalFilesStatesManager;
		private readonly ParserInterface _parser = new SrcMLParser();
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

		public void UpdateFile(ProjectItem item)
		{
			try
			{
				var path = item.FileNames[0];

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

			var parsed = _parser.Parse(filePath);
			foreach(var programElement in parsed)
			{
				var document = DocumentFactory.Create(programElement);
				_currentIndexer.AddDocument(document);
			}
			_indexFilesStatesManager.UpdateIndexFileState(filePath, indexFileState);
		}

	}
}
