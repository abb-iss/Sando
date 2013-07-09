using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.Core.Tools;
using Sando.DependencyInjection;

namespace Sando.Recommender
{
    internal class SwumQueriesSorter
    {
        private class ScoredQuery
        {
            public string Query { set; get; }
            public double Score { set; get; }

            internal ScoredQuery(string Query, double Score)
            {
                this.Query = Query;
                this.Score = Score;
            }
        }

        private abstract class AbstractQueryInputState
        {
            protected readonly string originalQuery;
            protected readonly string[] wordsInOriginalQuery;
            public abstract bool IsInState();
            protected abstract String[] InternalSortQueries(String[] queries);
            
            public String[] SortQueries(String[] queries)
            {
                queries = queries.Select(RemoveDupWords).Where(s => !s.Equals
                    (originalQuery.Trim(), StringComparison.InvariantCultureIgnoreCase)).
                        ToArray();
                queries = InternalSortQueries(queries);
                return queries;
            }

            private String RemoveDupWords(String input)
            {
                var list = new List<String>();
                var words = input.Split();
                for (var i = words.Count() - 1; i >= 0; i--)
                {
                    var word = words.ElementAt(i);
                    var subList = words.SubArray(0, i);
                    if (!subList.Contains(word, ToolHelpers.
                        GetCaseInsensitiveEqualityComparer()))
                    {
                        list.Insert(0, word);
                    }
                }
                return list.Aggregate((s1, s2) => s1 + " " + s2).Trim();
            }           

            protected AbstractQueryInputState(String originalQuery)
            {
                this.originalQuery = originalQuery;
                this.wordsInOriginalQuery = SplitQuery(originalQuery).ToArray();
            }

            protected String[] SelectQueriesByPrefixTerms(IEnumerable<string> queries, IEnumerable<string> startTerms)
            {
                var prefix = startTerms.Aggregate((s1, s2) => s1 + s2);
                return (from query in queries let removeSpace = query.Replace(" ", "") 
                    where removeSpace.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase) 
                        select query).ToArray();
            }


            protected String[] SelectQueriesByContainedTerms(IEnumerable<String> queries, IEnumerable<string> terms)
            {
                return queries.Where(q => SplitQuery(q).Any(t => terms.Contains(t, ToolHelpers.
                    GetCaseInsensitiveEqualityComparer()))).ToArray();
            }

            protected IEnumerable<string> SortQueriesByWordsCoOccurrence(IEnumerable<string> knownWords, 
                IEnumerable<string> queries, Func<string, IEnumerable<string>> GetWordsInQuery)
            {
                var list = new List<ScoredQuery>();
                knownWords = knownWords.ToList();
                foreach (var query in queries)
                {
                    var words = GetWordsInQuery(query).ToArray();
                    double averageCoOccur = CalculateAverageCoOccurrence(knownWords.ToArray(), words.ToArray());
                    list.Add(new ScoredQuery(query, averageCoOccur));
                }
                var sortedList = list.OrderByDescending(sq => sq.Score).ToArray();

                return sortedList.Select(sq => sq.Query).ToArray();
            }

            private double CalculateAverageCoOccurrence(String[] knownWords, String[] words)
            {
                var table = ServiceLocator.Resolve<DictionaryBasedSplitter>();
                double pairCount = knownWords.Count() * words.Count();
                double sum = 0.0;
                foreach (var word1 in words)
                {
                    foreach (var word2 in knownWords)
                    {
                        sum += table.GetCoOccurrenceCount(word1, word2);
                    }
                }
                return sum/pairCount;
            }


            protected IEnumerable<string> SplitQuery(String query)
            {
                var words = query.Split().Where(t => !String.IsNullOrWhiteSpace(t)).ToArray();
                return words.Any() ? words : new[] { String.Empty };
            }
        }

        private class FinishedWordInputState : AbstractQueryInputState
        {
            public FinishedWordInputState(string originalQuery)
                : base(originalQuery)
            {
            }

            public override bool IsInState()
            {
                return originalQuery.EndsWith(" ");
            }

            protected override string[] InternalSortQueries(string[] queries)
            {
                queries = SelectQueriesByContainedTerms(queries, wordsInOriginalQuery);

                var group1 = SelectQueriesByPrefixTerms(queries, wordsInOriginalQuery).ToList();
                var group2 = queries.Except(group1).ToList();

                group1 = SortQueriesByWordsCoOccurrence(wordsInOriginalQuery, group1, GetWordsInQuery).ToList();
                group2 = SortQueriesByWordsCoOccurrence(wordsInOriginalQuery, group2, GetWordsInQuery).ToList();
                group1.AddRange(group2);

                return group1.ToArray();
            }

            private IEnumerable<string> GetWordsInQuery(string q)
            {
                return SplitQuery(q).Except(wordsInOriginalQuery, ToolHelpers.
                        GetCaseInsensitiveEqualityComparer());
            }
        }

        private class NotFinishedWordInputState : AbstractQueryInputState
        {
            public NotFinishedWordInputState(string originalQuery)
                : base(originalQuery)
            {
            }

            public override bool IsInState()
            {
                return !originalQuery.EndsWith(" ") && !IsWordInDictionary(wordsInOriginalQuery.Last());
            }

            protected override string[] InternalSortQueries(string[] queries)
            {
                queries = wordsInOriginalQuery.Count() > 1 ? SelectQueriesByContainedTerms(queries, 
                    wordsInOriginalQuery.
                    SubArray(0, wordsInOriginalQuery.Count() - 1)) : queries;

                var group1 = SelectQueriesByPrefixTerms(queries, wordsInOriginalQuery).ToList();
                var group2 = queries.Except(group1).ToList();

                group1 = group1.OrderBy(q => q).ToList();

                group2 = wordsInOriginalQuery.Count() > 1 ?
                    SortQueriesByWordsCoOccurrence(wordsInOriginalQuery.SubArray(0, 
                        wordsInOriginalQuery.Count() - 1), queries, 
                            GetWordsInQuery).ToList() : group2.OrderBy(q => q).ToList();
   
                group1.AddRange(group2);
                return group1.ToArray();
            }

            private IEnumerable<string> GetWordsInQuery(string query)
            {
                var knownWords = wordsInOriginalQuery.Count() > 1 ? wordsInOriginalQuery.SubArray(0,
                    wordsInOriginalQuery.Count() - 1) : new String[] { };
                return SplitQuery(query).Except(knownWords, ToolHelpers.
                    GetCaseInsensitiveEqualityComparer());
            }
        }

        private class MiddleInputState : AbstractQueryInputState
        {
            public MiddleInputState(string originalQuery)
                : base(originalQuery)
            {
            }

            public override bool IsInState()
            {
                return !originalQuery.EndsWith(" ") && IsWordInDictionary(wordsInOriginalQuery.Last());
            }

            protected override string[] InternalSortQueries(string[] queries)
            {
                queries = wordsInOriginalQuery.Count() > 1 ? SelectQueriesByContainedTerms(queries, 
                    wordsInOriginalQuery.SubArray(0, wordsInOriginalQuery.Count() - 1)) : queries;

                var group1 = SelectQueriesByPrefixTerms(queries, wordsInOriginalQuery).
                    OrderBy(q => q).ToList();
                var group2 = queries.Except(group1).ToList();

                group2 = wordsInOriginalQuery.Count() > 1 ? SortQueriesByWordsCoOccurrence
                    (wordsInOriginalQuery.SubArray(0, wordsInOriginalQuery.Count() - 1), 
                        queries, GetWordsInQuery).ToList() : group2.OrderBy(q => q).ToList();
                
                group1.AddRange(group2);
                return group1.ToArray();
            }

            private IEnumerable<string> GetWordsInQuery(string query)
            {
                var knownWords = wordsInOriginalQuery.Count() > 1 ? wordsInOriginalQuery.
                    SubArray(0, wordsInOriginalQuery.Count() - 1) : new String[] {};
                return SplitQuery(query).Except(knownWords,
                    ToolHelpers.GetCaseInsensitiveEqualityComparer());
            }
        }
       
        private static bool IsWordInDictionary(String word)
        {
            var dictionary = ServiceLocator.Resolve<DictionaryBasedSplitter>();
            return dictionary.DoesWordExist(word, DictionaryOption.NoStemming);
        }

        public string[] SelectSortSwumRecommendations(string originalQuery, string[] queries)
        {
            var filters = new AllFilters();
            var list = GetSearchHistoryItemStartingWith(originalQuery);
            var state = GetQueryInputState(originalQuery);
            list.AddRange(state.SortQueries(queries));
            return HandleCornerCases(originalQuery, filters.FilterBadQueries(list.ToArray()));
        }

        private AbstractQueryInputState GetQueryInputState(String query)
        {
            var states = new AbstractQueryInputState[]
            {
                new NotFinishedWordInputState(query),
                new MiddleInputState(query), 
                new FinishedWordInputState(query)
            };
            return states.First(s => s.IsInState());
        }

        private List<string> GetSearchHistoryItemStartingWith(String prefix)
        {
            var history = ServiceLocator.Resolve<SearchHistory>();
            return history.GetSearchHistoryItems(item => item.SearchString.
                StartsWith(prefix)).Select(i => i.SearchString).ToList();
        }

        private interface IRecommendedQueryFilter
        {
            String[] FilterBadQueries(String[] queries);
        }

        private class AllFilters : IRecommendedQueryFilter
        {
            public string[] FilterBadQueries(string[] queries)
            {
                var filters = new IRecommendedQueryFilter[]
                {
                    new DuplicateQueriesFilter(),
                    new ContainingNonLocalWordsQueriesFilter()
                };
                return filters.Aggregate(queries, (current, filter) => 
                    filter.FilterBadQueries(current));
            }
        }


        private class DuplicateQueriesFilter : IRecommendedQueryFilter
        {
            public string[] FilterBadQueries(string[] queries)
            {
                var set = new HashSet<String>(queries, new StringEqualityComparer());
                return set.ToArray();
            }

            private class StringEqualityComparer : IEqualityComparer<string>
            {
                public bool Equals(string x, string y)
                {
                    return x.Trim().Equals(y.Trim(), 
                        StringComparison.InvariantCultureIgnoreCase);
                }

                public int GetHashCode(string obj)
                {
                    return 0;
                }
            }
        }

        private class ContainingNonLocalWordsQueriesFilter : IRecommendedQueryFilter
        {
            public string[] FilterBadQueries(string[] queries)
            {
                var badQueries = queries.Where(q => q.Trim().Contains(" ") && !AllWordsInDictionary(q));
                return queries.Except(badQueries).ToArray();
            }

            private bool AllWordsInDictionary(string s)
            {
                var words = s.Split();
                var dictionary = ServiceLocator.Resolve<DictionaryBasedSplitter>();
                return words.All(w => dictionary.DoesWordExist(w, DictionaryOption.NoStemming));
            }
        }

        private String[] HandleCornerCases(String original, String[] recommended)
        {
            var allHandlers = new ICornerCaseHandler[]
            {
                new PreferVariableWhenNoSpace()
            };
            var handlers = allHandlers.Where(h => h.IsCornerCase(original));
            return handlers.Any() ? handlers.First().Handle(recommended) : recommended;
        }

        private interface ICornerCaseHandler
        {
            bool IsCornerCase(String originalQuery);
            String[] Handle(String[] queries);
        }

        private class PreferVariableWhenNoSpace : ICornerCaseHandler
        {
            public bool IsCornerCase(string originalQuery)
            {
                return !originalQuery.TrimStart().Contains(" ");
            }

            public string[] Handle(string[] queries)
            {
                var variableList = new List<String>();
                var othersList = new List<String>();
                foreach (var query in queries)
                {
                    var list = query.Trim().Contains(" ") ? othersList : 
                        variableList;
                    list.Add(query);
                }
                variableList.AddRange(othersList);
                return variableList.ToArray();
            }
        }
    }
}
