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
    public class SearchCriteriaReformer
    {
        private const int TERM_MINIMUM_LENGTH = 2;

        public static void ReformSearchCriteria(SimpleSearchCriteria criteria)
        {
            var specialTerms = GetSpecialTerms(criteria.SearchTerms);
            var terms = criteria.SearchTerms.Where(t => !t.StartsWith("\"")||!t.EndsWith("\"")).Select(t => t.NormalizeText()).Distinct().ToList();
            var originalTerms = terms.ToList();
            var dictionarySplittedTerms = terms.SelectMany
                    (ServiceLocator.Resolve<DictionaryBasedSplitter>().
                        ExtractWords).Where(t => t.Length >= TERM_MINIMUM_LENGTH).ToList();

            terms.AddRange(dictionarySplittedTerms.Except(terms));
            var queries = GetReformedQuery(terms.Distinct()).ToList();
            if (queries.Count > 0)
            {
                LogEvents.AddSearchTermsToOriginal(queries.First());
                var query = queries.First();
                terms.AddRange(query.WordsAfterReform.Except(terms));
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
            terms.AddRange(specialTerms);
            criteria.SearchTerms = ConvertToSortedSet(terms);
        }

        private static String[] GetSpecialTerms(IEnumerable<string> searchTerms)
        {
            return searchTerms.Where(t => !t.NormalizeText().Equals(t, 
                StringComparison.InvariantCultureIgnoreCase)).ToArray();
        }

        private static SortedSet<String> ConvertToSortedSet(IEnumerable<string> list)
        {
            var set = new SortedSet<string>();
            foreach (var s in list)
            {
                set.Add(s);
            }
            return set;
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
            return reformer.ReformTermsSynchronously(words).Where(r => r.ReformedWords.Any(w => 
                w.Category != TermChangeCategory.NOT_CHANGED)).ToList();
        }
    }
}
