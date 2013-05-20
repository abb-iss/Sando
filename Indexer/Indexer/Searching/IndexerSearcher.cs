using System.Collections.Generic;
using Lucene.Net.Search;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer.Searching.Criteria;
using System.Linq;
using Sando.Indexer.Documents.Converters;
using Sando.Indexer.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Indexer.Searching
{
	public class IndexerSearcher : IIndexerSearcher
	{
		public IndexerSearcher()
		{
			_documentIndexer = ServiceLocator.Resolve<DocumentIndexer>();
		}

		public IEnumerable<CodeSearchResult> Search(SearchCriteria searchCriteria)
		{
			var penalizeClasses = false;
			var searchQueryString = searchCriteria.ToQueryString();
			if(searchCriteria as SimpleSearchCriteria != null && (searchCriteria as SimpleSearchCriteria).RequiresWildcards())
			{
				_documentIndexer.QueryParser.SetAllowLeadingWildcard(true);
				_documentIndexer.QueryParser.SetLowercaseExpandedTerms(false);
				penalizeClasses = true;
			}
			else
			{
				_documentIndexer.QueryParser.SetAllowLeadingWildcard(false);
				_documentIndexer.QueryParser.SetLowercaseExpandedTerms(true);
			}

			var query = _documentIndexer.QueryParser.Parse(searchQueryString);
			var hitsPerPage = searchCriteria.NumberOfSearchResultsReturned;
			var collector = TopScoreDocCollector.create(hitsPerPage, true);
			var documentTuples = _documentIndexer.Search(query, collector);
			var searchResults = documentTuples.Select(d => new CodeSearchResult(ConverterFromHitToProgramElement.Create(d.Item1).Convert(), penalizeClasses && d.Item1.GetField(SandoField.ProgramElementType.ToString()).StringValue().Equals(ProgramElementType.Class.ToString().ToLower()) ? d.Item2 / 10 : d.Item2));

			return searchResults;
		}

		private readonly DocumentIndexer _documentIndexer;
	}
}
