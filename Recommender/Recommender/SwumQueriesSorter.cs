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
            internal string Query { set; get; }
            internal int Score { set; get; }
            private String[] words;

            internal ScoredQuery(String Query)
            {
                this.Query = Query;
                this.Score = 0;
            }

            internal String[] Split()
            {
                return words ?? (words = new CamelIdSplitter().Split(Query));
            }

            internal void AddPoints(int points)
            {
                this.Score += points;
            }
        }

        private ScoredQuery ComputeScore(ScoredQuery query)
        {
            var dictionary = ServiceLocator.Resolve<DictionaryBasedSplitter>();
            var words = query.Split();
            for (var i = 0; i < words.Count(); i++)
            {
                var word1 = words.ElementAt(i);
                for (var j = i; j < words.Count(); j++)
                {
                    var word2 = words.ElementAt(j);
                    var points = dictionary.GetCoOccurrenceCount(word1, word2);
                    query.AddPoints(points);
                }
            }
            return query;
        }

        private abstract class IQueryInputState
        {
            protected string originalQuery;
            protected string[] words;
            public abstract bool IsInState();
            public abstract String[] SortQueries(String[] queries);

            protected IQueryInputState(String originalQuery)
            {
                this.originalQuery = originalQuery;
                this.words = SplitQuery(originalQuery).ToArray();
            }
        }

        private class FinishedWordInputState : IQueryInputState
        {
            public FinishedWordInputState(string originalQuery)
                : base(originalQuery)
            {
            }

            public override bool IsInState()
            {
                return originalQuery.EndsWith(" ");
            }

            public override string[] SortQueries(string[] queries)
            {
                return null;
            }
        }

        private class NotFinishedWordInputState : IQueryInputState
        {
            public NotFinishedWordInputState(string originalQuery)
                : base(originalQuery)
            {
            }

            public override bool IsInState()
            {
                return !originalQuery.EndsWith(" ") && !IsWordInDictionary(words.Last());
            }

            public override string[] SortQueries(string[] queries)
            {
                throw new NotImplementedException();
            }
        }

        private class MiddleInputState : IQueryInputState
        {
            public MiddleInputState(string originalQuery)
                : base(originalQuery)
            {
            }

            public override bool IsInState()
            {
                return !originalQuery.EndsWith(" ") && IsWordInDictionary(words.Last());
            }

            public override string[] SortQueries(string[] queries)
            {
                throw new NotImplementedException();
            }
        }

        private static IEnumerable<string> SplitQuery(String query)
        {
            var words = query.Split();
            return words.Any() ? words : new[] { "" };
        }

        private static bool IsWordInDictionary(String word)
        {
            var dictionary = ServiceLocator.Resolve<DictionaryBasedSplitter>();
            return dictionary.DoesWordExist(word, DictionaryOption.NoStemming);
        }

        public string[] SortSwumRecommendations(string originalQuery, string[] queries)
        {
            var groups = queries.Select(q => new ScoredQuery(q)).GroupBy(q => q.Split().Count());
            return groups.SelectMany(SortGroup).Select(q => q.Query).ToArray();
        }

        private IEnumerable<ScoredQuery> SortGroup(IGrouping<int, ScoredQuery> group)
        {
            return group.Select(ComputeScore).OrderBy(q => -q.Score);
        }
    }
}
