using System;
using System.Collections.Generic;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Sando.Core;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer.Documents;
using Sando.Indexer.Searching.Criteria;

namespace Sando.Indexer.Searching
{
	public class IndexerSearcher: IIndexerSearcher
	{
		public IndexerSearcher(SolutionKey solutionKey)
		{
			documentIndexer = DocumentIndexerFactory.CreateIndexer(solutionKey, AnalyzerType.Snowball);
		}

		public List<Tuple<ProgramElement, float>> Search(SearchCriteria searchCriteria)
		{
			string searchQueryString = searchCriteria.ToQueryString();
			Query query = documentIndexer.QueryParser.Parse(searchQueryString);
            int hitsPerPage = searchCriteria.NumberOfSearchResultsReturned;
		    return ExecuteSearch(query, hitsPerPage);
		}

        public List<Tuple<ProgramElement, float>> SearchNoAnalyzer(SearchCriteria searchCriteria)
        {
            var query = new BooleanQuery();
            int hitsPerPage = searchCriteria.NumberOfSearchResultsReturned;
            var locations = (searchCriteria as SimpleSearchCriteria).Locations.GetEnumerator();
            locations.MoveNext();
            var filePath = new Term("FullFilePath", SandoDocument.StandardizeFilePath(locations.Current));
            query.Add(new TermQuery(filePath), BooleanClause.Occur.MUST);
            foreach (var myType in (searchCriteria as SimpleSearchCriteria).ProgramElementTypes)
            {
                var elementType = new Term(SandoField.ProgramElementType.ToString(), myType.ToString().ToLower());
                query.Add(new TermQuery(elementType), BooleanClause.Occur.SHOULD);

            }
            return ExecuteSearch(query, hitsPerPage);

        }

	    private List<Tuple<ProgramElement, float>> ExecuteSearch(Query query, int hitsPerPage)
        {        			
			TopScoreDocCollector collector = TopScoreDocCollector.create(hitsPerPage, true);
			documentIndexer.IndexSearcher.Search(query, collector);

			ScoreDoc[] hits = collector.TopDocs().ScoreDocs;

			var searchResults = new List<Tuple<ProgramElement, float>>();

            //var explain = documentIndexer.IndexSearcher.Explain(query, hits[0].doc);
            //var explain2 = documentIndexer.IndexSearcher.Explain(query, hits[1].doc);

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
