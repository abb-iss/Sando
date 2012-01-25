using System;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Snowball;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Sando.Indexer.Documents;

namespace Sando.Indexer
{
	public class DocumentIndexer : IDisposable
	{
		public DocumentIndexer(string luceneTempIndexesDirectory, Analyzer analyzer)
		{
			System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(luceneTempIndexesDirectory);
			LuceneIndexesDirectory = FSDirectory.Open(directoryInfo);
			Analyzer = analyzer;
			IndexWriter = new IndexWriter(LuceneIndexesDirectory, analyzer, IndexWriter.MaxFieldLength.LIMITED);
		}

		public virtual void AddDocument(SandoDocument sandoDocument)
		{
			IndexWriter.AddDocument(sandoDocument.GetDocument());
		}

		public void CommitChanges()
		{
			IndexWriter.Commit();
		}

		public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
		
		protected virtual void Dispose(bool disposing)
        {
            if(!this.disposed)
            {
                if(disposing)
                {
					IndexWriter.Close();
					LuceneIndexesDirectory.Close();
                }

                disposed = true;
            }
        }

        ~DocumentIndexer()
        {
            Dispose(false);
        }

		protected virtual Directory LuceneIndexesDirectory { get; set; }
		protected virtual Analyzer Analyzer { get; set; }
		protected virtual IndexWriter IndexWriter { get; set; }

		private bool disposed = false;
	}

	public enum AnalyzerType
	{
		Simple,Snowball,Standard
	}

	public class DocumentIndexerFactory
	{
		public static DocumentIndexer CreateIndexer(string luceneTempIndexesDirectory, AnalyzerType analyzerType)
		{
			switch (analyzerType)
			{
				case AnalyzerType.Simple: 
					return new DocumentIndexer(luceneTempIndexesDirectory,new SimpleAnalyzer());
				case AnalyzerType.Snowball:
					return new DocumentIndexer(luceneTempIndexesDirectory, new SnowballAnalyzer("English"));
				case AnalyzerType.Standard:
					return new DocumentIndexer(luceneTempIndexesDirectory, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));
			}
			return new DocumentIndexer(luceneTempIndexesDirectory,new SimpleAnalyzer());
		}
		
	}
}
