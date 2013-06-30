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
            public int Score { set; get; }

            internal ScoredQuery(string Query, int Score)
            {
                this.Query = Query;
                this.Score = Score;
            }
        }


        private abstract class AbstractQueryInputState
        {
            protected string originalQuery;
            protected string[] wordsInOriginalQuery;
            public abstract bool IsInState();
            protected abstract String[] InternalSortQueries(String[] queries);
            
            public String[] SortQueries(String[] queries)
            {
                queries = InternalSortQueries(queries);
                return queries;
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

            protected String[] SortQueriesByWordsCoOccurrence(IEnumerable<string> knownWords, 
                IEnumerable<string> queries, Func<string, string> GetWordInQuery)
            {
                var list = new List<ScoredQuery>();
                knownWords = knownWords.ToList();
                var table = ServiceLocator.Resolve<DictionaryBasedSplitter>();
                foreach (var query in queries)
                {
                    var word = GetWordInQuery(query);
                    int cooccur = knownWords.Sum(km => table.GetCoOccurrenceCount(word, km));
                    list.Add(new ScoredQuery(query, cooccur));
                }
                return list.OrderByDescending(sq => sq.Score).Select(sq => sq.Query).ToArray();
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
                queries = SelectQueriesByPrefixTerms(queries, wordsInOriginalQuery);
                queries = SortQueriesByWordsCoOccurrence(wordsInOriginalQuery, queries, GetWordInQuery);
                return queries;
            }

            private string GetWordInQuery(string q)
            {
                var last = SplitQuery(q).Last();
                return wordsInOriginalQuery.Contains(last) ? String.Empty : last;
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
                queries = wordsInOriginalQuery.Count() > 1 ? SelectQueriesByPrefixTerms(queries, wordsInOriginalQuery.
                    SubArray(0, wordsInOriginalQuery.Count() - 1)) : queries;

                var group1 = SelectQueriesByPrefixTerms(queries, wordsInOriginalQuery).ToList();
                var group2 = queries.Except(group1).ToList();

                group1 = group1.OrderBy(q => q).ToList();

                if (wordsInOriginalQuery.Count() > 1)
                {
                    group2 = SortQueriesByWordsCoOccurrence(wordsInOriginalQuery.SubArray(0, 
                        wordsInOriginalQuery.Count() - 1), queries, GetWordInQuery).ToList();
                }

                group1.AddRange(group2);
                return group1.ToArray();
            }

            private string GetWordInQuery(string query)
            {
                return SplitQuery(query).Last();
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
                queries = wordsInOriginalQuery.Count() > 1 ? SelectQueriesByPrefixTerms(queries, wordsInOriginalQuery.
                    SubArray(0, wordsInOriginalQuery.Count() - 1)) : queries;

                var group1 = SelectQueriesByPrefixTerms(queries, wordsInOriginalQuery).OrderBy(q => q).ToList();
                var group2 = queries.Except(group1).ToList();

                if (wordsInOriginalQuery.Count() > 1)
                {
                    group2 = SortQueriesByWordsCoOccurrence(wordsInOriginalQuery.SubArray(0,
                        wordsInOriginalQuery.Count() - 1), queries, GetWordInQuery).ToList();
                }

                group1.AddRange(group2);
                return group1.ToArray();
            }

            private string GetWordInQuery(string query)
            {
                return SplitQuery(query).Last();
            }
        }
       
        private static bool IsWordInDictionary(String word)
        {
            var dictionary = ServiceLocator.Resolve<DictionaryBasedSplitter>();
            return dictionary.DoesWordExist(word, DictionaryOption.NoStemming);
        }

        public string[] SelectSortSwumRecommendations(string originalQuery, string[] queries)
        {
            var state = GetQueryInputState(originalQuery);
            return state.SortQueries(queries);
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
    }
}
