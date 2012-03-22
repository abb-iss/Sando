using System;
using System.Collections.Generic;
using Lucene.Net.Search;
using Sando.Core;
using Sando.Indexer.Documents;
using Sando.Indexer.Searching.Criteria;

namespace Sando.Indexer.Searching
{
	public class IndexerSearcher: IIndexerSearcher
	{
		public IndexerSearcher(SolutionKey solutionKey)
		{
			documentIndexer = DocumentIndexerFactory.CreateIndexer(solutionKey, AnalyzerType.Default);
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
				ProgramElement programElement = ProgramElementReader.ReadProgramElementFromDocument(hitDocument);
				searchResults.Add(Tuple.Create(programElement, score));
			}
			return searchResults;
		}

		private DocumentIndexer documentIndexer;
	}

	public class IndexerSearcherFactory
	{
		public static IIndexerSearcher CreateSearcher(SolutionKey solutionKey)
		{
			return new IndexerSearcher(solutionKey);	
		}
	}
}
