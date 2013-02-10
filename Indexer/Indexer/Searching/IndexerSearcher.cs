using System;
using System.Collections.Generic;
using Lucene.Net.Search;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer.Searching.Criteria;

namespace Sando.Indexer.Searching
{
	public class IndexerSearcher: IIndexerSearcher
	{
		public IndexerSearcher()
		{
			_documentIndexer = ServiceLocator.Resolve<DocumentIndexer>();
		}

		public List<Tuple<ProgramElement, float>> Search(SearchCriteria searchCriteria)
		{
			string searchQueryString = searchCriteria.ToQueryString();
			Query query = _documentIndexer.QueryParser.Parse(searchQueryString);
			int hitsPerPage = searchCriteria.NumberOfSearchResultsReturned;
			TopScoreDocCollector collector = TopScoreDocCollector.create(hitsPerPage, true);
			_documentIndexer.IndexSearcher.Search(query, collector);

			ScoreDoc[] hits = collector.TopDocs().ScoreDocs;

			var searchResults = new List<Tuple<ProgramElement, float>>();

			for(int i = 0; i < hits.Length; i++)
			{
				var hitDocument = _documentIndexer.IndexSearcher.Doc(hits[i].doc);
				var score = hits[i].score;
				ProgramElement programElement = ProgramElementReader.ReadProgramElementFromDocument(hitDocument);
				searchResults.Add(Tuple.Create(programElement, score));
			}
			return searchResults;
		}

		private readonly DocumentIndexer _documentIndexer;
	}
}
