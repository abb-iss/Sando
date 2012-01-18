using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Sando.Indexer.Configuration;
using Sando.Indexer.Documents;

namespace Sando.Indexer
{
	public class DocumentIndexer
	{
		public DocumentIndexer(Analyzer analyzer)
		{
			LuceneIndexesDirectory = FSDirectory.GetDirectory(IndexerConfiguration.GetValue("LuceneIndexesDirectory"));
			Analyzer = analyzer;
			IndexWriter = new IndexWriter(LuceneIndexesDirectory, analyzer);
		}

		public virtual void AddDocument(SandoDocument sandoDocument)
		{
			IndexWriter.AddDocument(sandoDocument.GetDocument());
		}

		protected virtual Directory LuceneIndexesDirectory { get; set; }
		protected virtual Analyzer Analyzer { get; set; }
		protected virtual IndexWriter IndexWriter { get; set; }
	}
}
