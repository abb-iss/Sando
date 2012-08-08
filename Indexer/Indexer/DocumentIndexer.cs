using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Snowball;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Sando.Core;
using Sando.Core.Extensions.Logging;
using Sando.Indexer.Documents;
using Sando.Indexer.Exceptions;
using Sando.Translation;

namespace Sando.Indexer
{
	public class DocumentIndexer : IDisposable
	{
		public DocumentIndexer(string luceneTempIndexesDirectory, Analyzer analyzer)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(luceneTempIndexesDirectory), "DocumentIndexer:Constructor - luceneTempIndexesDirectory cannot be null or an empty string!");
			Contract.Requires(System.IO.Directory.Exists(luceneTempIndexesDirectory), "DocumentIndexer:Constructor - luceneTempIndexesDirectory does not point to a valid directory!");
			Contract.Requires(analyzer != null, "DocumentIndexer:Constructor - analyzer cannot be null!");

			try
			{
				System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(luceneTempIndexesDirectory);
				LuceneIndexesDirectory = FSDirectory.Open(directoryInfo);
				Analyzer = analyzer;
				IndexWriter = new IndexWriter(LuceneIndexesDirectory, analyzer, IndexWriter.MaxFieldLength.LIMITED);
				IndexSearcher = new IndexSearcher(LuceneIndexesDirectory, true);
				QueryParser = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, Configuration.Configuration.GetValue("DefaultSearchFieldName"), analyzer);
				indexUpdateListeners = new List<IIndexUpdateListener>();
			}
			catch(CorruptIndexException corruptIndexEx)
			{
                FileLogger.DefaultLogger.Error(ExceptionFormatter.CreateMessage(corruptIndexEx));
				throw new IndexerException(TranslationCode.Exception_Indexer_LuceneIndexIsCorrupt, corruptIndexEx);
			}
			catch(LockObtainFailedException lockObtainFailedEx)
			{
                FileLogger.DefaultLogger.Error(ExceptionFormatter.CreateMessage(lockObtainFailedEx));
				throw new IndexerException(TranslationCode.Exception_Indexer_LuceneIndexAlreadyOpened, lockObtainFailedEx);
			}
			catch(System.IO.IOException ioEx)
			{
                FileLogger.DefaultLogger.Error(ExceptionFormatter.CreateMessage(ioEx));
				throw new IndexerException(TranslationCode.Exception_General_IOException, ioEx, ioEx.Message);
			}
		}

        [MethodImpl(MethodImplOptions.Synchronized)]
		public virtual void AddDocument(SandoDocument sandoDocument)
		{
			Contract.Requires(sandoDocument != null, "DocumentIndexer:AddDocument - sandoDocument cannot be null!");
            
            IndexWriter.AddDocument(sandoDocument.GetDocument());
		}

        [MethodImpl(MethodImplOptions.Synchronized)]
        public virtual void DeleteDocuments(string fullFilePath)
        {
            if (String.IsNullOrWhiteSpace(fullFilePath))
                return;
            var term = new Term("FullFilePath", SandoDocument.StandardizeFilePath(fullFilePath));
            IndexWriter.DeleteDocuments(new TermQuery(term));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
		public void CommitChanges()
		{
			IndexWriter.Commit();
			UpdateReader();
			NotifyIndexUpdateListeners();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ClearIndex()
		{   
            IndexWriter.GetDirectory().EnsureOpen();
			IndexWriter.DeleteAll();
		}

		private void UpdateReader()
		{
			//TODO - please check this out and see if you agree tha we need this
			var oldReader = IndexSearcher.GetIndexReader();
			IndexReader newReader = oldReader.Reopen(true);
			if (newReader != IndexSearcher.GetIndexReader())
			{
				oldReader.Close();
				IndexSearcher = new IndexSearcher(newReader);
			}
		}


		public void AddIndexUpdateListener(IIndexUpdateListener indexUpdateListener)
		{
			this.indexUpdateListeners.Add(indexUpdateListener);
		}

		public void RemoveIndexUpdateListener(IIndexUpdateListener indexUpdateListener)
		{
			this.indexUpdateListeners.Remove(indexUpdateListener);
		}

		private void NotifyIndexUpdateListeners()
		{
			foreach(IIndexUpdateListener listener in this.indexUpdateListeners)
			{
				listener.NotifyAboutIndexUpdate();
			}
		}


		public bool IsUsable()
		{
            if (this.disposed)
                return false;
            try
            {
                this.IndexSearcher.Search(new TermQuery(new Term("asdf")));
            }catch(AlreadyClosedException)
            {
                return false;
            }
		    return true;
		}

        public void Dispose()
        {
            Dispose(false);
        }


		public void Dispose(bool killReaders)
        {
            Dispose(true,killReaders);
            GC.SuppressFinalize(this);
        }
		
		protected virtual void Dispose(bool disposing, bool killReaders)
        {
            if(!this.disposed)
            {
                if(disposing)
                {
					IndexWriter.Close();
					IndexReader indexReader = IndexSearcher.GetIndexReader();
                    if(indexReader != null && killReaders)
                        indexReader.Close();
					IndexSearcher.Close();
					LuceneIndexesDirectory.Close();
                }

                disposed = true;
            }
        }

        ~DocumentIndexer()
        {
            Dispose(false);
        }

		public virtual Directory LuceneIndexesDirectory { get; set; }
		public virtual IndexSearcher IndexSearcher { get; protected set; }
		public virtual QueryParser QueryParser { get; protected set; }
		protected virtual Analyzer Analyzer { get; set; }
		protected virtual IndexWriter IndexWriter { get; set; }

		private List<IIndexUpdateListener> indexUpdateListeners;
		private bool disposed = false;
	}

	public enum AnalyzerType
	{
		Simple, Snowball, Standard, Default
	}

	public class DocumentIndexerFactory
	{
		public static DocumentIndexer CreateIndexer(SolutionKey solutionKey, AnalyzerType analyzerType)
		{
			Guid solutionId = solutionKey.GetSolutionId();
			if(documentIndexers.ContainsKey(solutionId))
			{
				if(!documentIndexers[solutionId].IsUsable())
				{
					documentIndexers[solutionId] = CreateInstance(solutionKey.GetIndexPath(), analyzerType);
				}
			}
			else
			{
			    DocumentIndexer documentIndexer = CreateInstance(solutionKey.GetIndexPath(), analyzerType);
			    documentIndexers.Add(solutionId, documentIndexer);
			}
		    return documentIndexers[solutionId];
		}

		private static DocumentIndexer CreateInstance(string luceneIndex, AnalyzerType analyzerType)
		{
			switch(analyzerType)
			{
				case AnalyzerType.Simple:
					return new DocumentIndexer(luceneIndex, new SimpleAnalyzer());
				case AnalyzerType.Snowball:
					return new DocumentIndexer(luceneIndex, new SnowballAnalyzer("English"));
				case AnalyzerType.Standard:
					return new DocumentIndexer(luceneIndex, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));
				case AnalyzerType.Default:
				default:
					return new DocumentIndexer(luceneIndex, new SimpleAnalyzer());
			}			
		}

		private static Dictionary<Guid, DocumentIndexer> documentIndexers = new Dictionary<Guid, DocumentIndexer>();
	}
}
