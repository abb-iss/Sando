using System.Collections.Generic;
using Lucene.Net.Search;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer.Searching.Criteria;
using System.Linq;
using Sando.Indexer.Documents.Converters;

namespace Sando.Indexer.Searching
{
	public class IndexerSearcher: IIndexerSearcher
	{
		public IndexerSearcher()
		{
			_documentIndexer = ServiceLocator.Resolve<DocumentIndexer>();
		}

        public IEnumerable<CodeSearchResult> Search(SearchCriteria searchCriteria)
		{
			var searchQueryString = searchCriteria.ToQueryString();
			var query = _documentIndexer.QueryParser.Parse(searchQueryString);
			var hitsPerPage = searchCriteria.NumberOfSearchResultsReturned;
			var collector = TopScoreDocCollector.create(hitsPerPage, true);
			var documentTuples = _documentIndexer.Search(query, collector);
		    var searchResults = documentTuples.Select(d => new CodeSearchResult(ConverterFromHitToProgramElement.Create(d.Item1).Convert(), d.Item2));
			return searchResults;
		}

		private readonly DocumentIndexer _documentIndexer;
	}
}
