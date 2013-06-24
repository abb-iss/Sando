using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.Core.QueryRefomers;
using Sando.Core.Tools;
using Sando.DependencyInjection;

namespace Sando.Indexer.Searching.Criteria
{
    internal class SearchCriteriaReformer
    {
        public static void ReformSearchCriteria(SimpleSearchCriteria criteria, List<String> terms)
        {
            var originalTerms = terms.ToList();
            var dictionarySplittedTerms = terms.SelectMany(ServiceLocator.Resolve<DictionaryBasedSplitter>
                ().ExtractWords).ToList();
            terms.AddRange(dictionarySplittedTerms);
            var queries = GetReformedQuery(terms.Distinct()).ToList();
            if (queries.Count > 0)
            {
                var query = queries.First();
                terms.AddRange(query.ReformedTerms.ToList());
                criteria.Explanation = GetExplanation(query, originalTerms);
                criteria.Reformed = true;
                criteria.RecommendedQueries =
                    queries.GetRange(1, queries.Count - 1).Select(n => n.QueryString).AsQueryable();
            }
            else
            {
                criteria.Explanation = String.Empty;
                criteria.Reformed = false;
                criteria.RecommendedQueries = Enumerable.Empty<String>().AsQueryable();
            }
        }

        private static String GetExplanation(IReformedQuery query, List<String> originalTerms)
        {
            var sb = new StringBuilder();
            sb.Append("Added search terms:");
            foreach (var term in query.ReformedQuery)
            {
                if (!originalTerms.Contains(term.NewTerm))
                {
                    sb.Append(term.NewTerm + ", ");
                }
            }
            return sb.ToString().TrimEnd(new char[]{',', ' '}) + ". ";
        }

        private static IEnumerable<IReformedQuery> GetReformedQuery(IEnumerable<string> words)
        {
            words = words.ToList();
            var reformer = ServiceLocator.Resolve<QueryReformerManager>();
            return reformer.ReformTermsSynchronously(words).ToList();
        }

    }
}
