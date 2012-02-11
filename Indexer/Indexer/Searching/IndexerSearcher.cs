using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Sando.Core;
using Sando.Indexer.Searching.Criteria;
using Lucene.Net.QueryParsers;

namespace Sando.Indexer.Searching
{
	public class IndexerSearcher: IIndexerSearcher
	{
		public IndexerSearcher(String directoryPath)
		{
			documentIndexer = DocumentIndexerFactory.CreateIndexer(directoryPath, AnalyzerType.Default);
		}

		public List<Tuple<ProgramElement, float>> Search(SearchCriteria searchCriteria)
		{
			string searchQueryString = searchCriteria.ToQueryString();
			Query query = documentIndexer.QueryParser.Parse(searchQueryString);
			int hitsPerPage = 10;
			TopScoreDocCollector collector = TopScoreDocCollector.create(hitsPerPage, true);
			documentIndexer.IndexSearcher.Search(query, collector);

			ScoreDoc[] hits = collector.TopDocs().ScoreDocs;

			var searchResults = new List<Tuple<ProgramElement, float>>();

			for(int i = 0; i < hits.Length; i++)
			{
				var hitDocument = documentIndexer.IndexSearcher.Doc(hits[i].doc);
				var score = hits[i].score;
				MethodElement methodElement = new MethodElement(
						accessLevel: AccessLevel.Public,
						arguments: String.Empty,
						body: "the body",
						classId: Guid.NewGuid(),
						definitionLineNumber: 0,
						fullFilePath: "full path",
						name: hitDocument.GetField("Name").StringValue(),
						returnType: "Object",
						snippet: "public Object " + hitDocument.GetField("Name").StringValue() + "(){the body}"
					);
				searchResults.Add(Tuple.Create(methodElement as ProgramElement, score));
			}
			return searchResults;
		}

		private DocumentIndexer documentIndexer;
	}

	public class IndexerSearcherFactory
	{
		public static IIndexerSearcher CreateSearcher(String directoryPath)
		{
			return new IndexerSearcher(directoryPath);	
		}
	}
}
