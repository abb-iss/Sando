using System;
using System.Collections.Generic;
using Lucene.Net.Search;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ProgramElementContracts;
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

        public IEnumerable<Tuple<ProgramElement, float>> Search(SearchCriteria searchCriteria)
		{
			string searchQueryString = searchCriteria.ToQueryString();
			Query query = _documentIndexer.QueryParser.Parse(searchQueryString);
			int hitsPerPage = searchCriteria.NumberOfSearchResultsReturned;
			TopScoreDocCollector collector = TopScoreDocCollector.create(hitsPerPage, true);
			var documentTuples = _documentIndexer.Search(query, collector);
		    var searchResults =
		        documentTuples.Select(
		            d =>
		            new Tuple<ProgramElement, float>(ConverterFromHitToProgramElement.Create(d.Item1).Convert(), d.Item2));
			return searchResults;
		}

		private readonly DocumentIndexer _documentIndexer;
	}
}
