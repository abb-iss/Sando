using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Sando.Core;
using Sando.Core.Extensions.Logging;
using Sando.DependencyInjection;
using Sando.Indexer.Documents;
using Sando.Indexer.Exceptions;
using Sando.Translation;

namespace Sando.Indexer
{
	public class DocumentIndexer : IDisposable
	{
		public DocumentIndexer()
		{
			try
			{
                var solutionKey = ServiceLocator.Resolve<SolutionKey>();
			
                var directoryInfo = new System.IO.DirectoryInfo(solutionKey.IndexPath);
				LuceneIndexesDirectory = FSDirectory.Open(directoryInfo);
				Analyzer = ServiceLocator.Resolve<Analyzer>();
                IndexWriter = new IndexWriter(LuceneIndexesDirectory, Analyzer, IndexWriter.MaxFieldLength.LIMITED);
				IndexSearcher = new IndexSearcher(LuceneIndexesDirectory, true);
                QueryParser = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, Configuration.Configuration.GetValue("DefaultSearchFieldName"), Analyzer);
				_indexUpdateListeners = new List<IIndexUpdateListener>();
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
			_indexUpdateListeners.Add(indexUpdateListener);
		}

		public void RemoveIndexUpdateListener(IIndexUpdateListener indexUpdateListener)
		{
			_indexUpdateListeners.Remove(indexUpdateListener);
		}

		private void NotifyIndexUpdateListeners()
		{
			foreach(var listener in _indexUpdateListeners)
			{
				listener.NotifyAboutIndexUpdate();
			}
		}


		public bool IsUsable()
		{
            if (_disposed)
                return false;
            try
            {
                IndexSearcher.Search(new TermQuery(new Term("asdf")), 1);
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
            Dispose(true, killReaders);
            GC.SuppressFinalize(this);
        }
		
		protected virtual void Dispose(bool disposing, bool killReaders)
        {
            if(!_disposed)
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

                _disposed = true;
            }
        }

        ~DocumentIndexer()
        {
            Dispose(false);
        }

		public Directory LuceneIndexesDirectory { get; set; }
		public IndexSearcher IndexSearcher { get; protected set; }
		public QueryParser QueryParser { get; protected set; }
		protected Analyzer Analyzer { get; set; }
		protected IndexWriter IndexWriter { get; set; }

		private readonly List<IIndexUpdateListener> _indexUpdateListeners;
		private bool _disposed;
    }

	public enum AnalyzerType
	{
		Simple, Snowball, Standard, Default
	}
}
