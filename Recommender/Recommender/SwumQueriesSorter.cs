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
            for (int i = 0; i < words.Count(); i++)
            {
                var word1 = words.ElementAt(i);
                for (int j = i; j < words.Count(); j++)
                {
                    var word2 = words.ElementAt(j);
                    var points = dictionary.GetCoOccurrenceCount(word1, word2);
                    query.AddPoints(points);
                }
            }
            return query;
        }

        public String[] SortSwumRecommendations(String[] queries)
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
