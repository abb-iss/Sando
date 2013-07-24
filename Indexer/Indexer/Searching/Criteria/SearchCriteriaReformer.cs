using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.Core.Logging.Events;
using Sando.Core.QueryRefomers;
using Sando.Core.Tools;
using Sando.DependencyInjection;

namespace Sando.Indexer.Searching.Criteria
{
    internal class SearchCriteriaReformer
    {
        private const int TERM_MINIMUM_LENGTH = 2;

        public static void ReformSearchCriteria(SimpleSearchCriteria criteria, List<String> terms)
        {
            var originalTerms = terms.ToList();
            var dictionarySplittedTerms = terms.SelectMany
                    (ServiceLocator.Resolve<DictionaryBasedSplitter>().
                        ExtractWords).Where(t => t.Length >= TERM_MINIMUM_LENGTH).ToList();
            terms.AddRange(dictionarySplittedTerms);
            var queries = GetReformedQuery(terms.Distinct()).ToList();
            if (queries.Count > 0)
            {
                LogEvents.AddSearchTermsToOriginal(queries.First());
                var query = queries.First();
                terms.AddRange(query.WordsAfterReform.ToList());
                criteria.Explanation = GetExplanation(query, originalTerms);
                criteria.Reformed = true;
                criteria.RecommendedQueries = queries.GetRange(1, queries.Count - 1).
                    Select(n => n.QueryString).AsQueryable();
                if (queries.Count > 1)
                {
                    LogEvents.IssueRecommendedQueries(queries.GetRange(1, queries.Count - 1).
                        ToArray());
                }
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
            var appended = false;
            var sb = new StringBuilder();
            sb.Append("Added search term(s):");
            foreach (var term in query.ReformedWords.Where(term => !originalTerms.Contains(term.NewTerm)))
            {
                appended = true;
                sb.Append(" " + term.NewTerm + ", ");
            }
            return appended ? sb.ToString().TrimEnd(new char[]{',', ' '}) + ". " : String.Empty;
        }

        private static IEnumerable<IReformedQuery> GetReformedQuery(IEnumerable<string> words)
        {
            words = words.ToList();
            var reformer = ServiceLocator.Resolve<QueryReformerManager>();
            return reformer.ReformTermsSynchronously(words).ToList();
        }

    }
}
