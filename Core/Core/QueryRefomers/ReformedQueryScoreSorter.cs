using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.QueryRefomers
{
    internal partial class ReformedQuerySorters
    {
        private class ScoreBasedSorter : IReformedQuerySorter
        {
            private class ScoredReformedQuery : IComparable<ScoredReformedQuery>
            {
                internal IReformedQuery Query { private set; get; }
                internal int Score { private set; get; }

                internal ScoredReformedQuery(IReformedQuery query)
                {
                    this.Query = query;
                    this.Score = 0;
                }

                internal void AddPoints(int points)
                {
                    this.Score += points;
                }
                
                public int CompareTo(ScoredReformedQuery other)
                {
                    return this.Score.CompareTo(other.Score);
                }
            }

            private class ScoreGenerator
            {
                private int currentScore;
                private readonly int interval;

                internal ScoreGenerator(int interval)
                {
                    this.currentScore = 0;
                    this.interval = interval;
                }

                internal int GetNextScore()
                {
                    currentScore += interval;
                    return currentScore;
                }

            }

            private const int COOCCURRENCE_INTERVAL = 1;
            private const int EDIT_DISTANCE_INTERVAL = 1;
            private const int SE_SYNONYM_COUNT_INTERVAL = 4;
            private const int GENERAL_SYNONYM_COUNT_INTERVAL = 3;

            public IEnumerable<IReformedQuery> SortReformedQueries(IEnumerable<IReformedQuery> queries)
            {
                var scoredQuery = queries.Select(q => new ScoredReformedQuery(q)).ToList();
                AddScoreToGroups(scoredQuery.GroupBy(q => q.Query.CoOccurrenceCount).OrderBy(q => q.Key),
                    new ScoreGenerator(COOCCURRENCE_INTERVAL));
                AddScoreToGroups(scoredQuery.GroupBy(q => q.Query.EditDistance).OrderBy(q => -q.Key),
                    new ScoreGenerator(EDIT_DISTANCE_INTERVAL));
                AddScoreToGroups(GroupScoredQueriesByTermChangeTypeCount(scoredQuery,
                    TermChangeCategory.SE_SYNONYM), new ScoreGenerator(SE_SYNONYM_COUNT_INTERVAL));
                AddScoreToGroups(GroupScoredQueriesByTermChangeTypeCount(scoredQuery,
                    TermChangeCategory.GENERAL_SYNONYM), new ScoreGenerator(GENERAL_SYNONYM_COUNT_INTERVAL));
                return scoredQuery.OrderBy(sq => -sq.Score).Select(sq => sq.Query);
            }

            private IEnumerable<IGrouping<int, ScoredReformedQuery>> GroupScoredQueriesByTermChangeTypeCount
                (IEnumerable<ScoredReformedQuery> scoredQueries, TermChangeCategory type)
            {
                return scoredQueries.GroupBy(sq => sq.Query.ReformedWords.Count(w => w.Category == type));
            }


            private void AddScoreToGroups(IEnumerable<IGrouping<int, ScoredReformedQuery>> groups, 
                ScoreGenerator scoreGenerator)
            {
                foreach (var group in groups)
                {
                    var points = scoreGenerator.GetNextScore();
                    foreach (var query in group)
                    {
                        query.AddPoints(points);
                    }
                }
            }
        }
    }
}
