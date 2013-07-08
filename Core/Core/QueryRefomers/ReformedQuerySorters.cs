using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.QueryRefomers
{
    internal enum QuerySorterType
    {
        EDIT_DISTANCE,
        COOCCURRENCE,
        ROBIN_HOOD,
        SCORE,
        NULL_SORTER,
    }


    internal partial class ReformedQuerySorters
    {
        private class EditDistanceSorter : IReformedQuerySorter
        {
            public IEnumerable<IReformedQuery> SortReformedQueries(IEnumerable<IReformedQuery> queries)
            {
                return queries.OrderBy(GetTotalDistance);
            }

            private int GetTotalDistance(IReformedQuery query)
            {
                return query.ReformedWords.Sum(term => term.DistanceFromOriginal);
            }
        }

        private class CoOccurrenceSorter : IReformedQuerySorter
        {
            public IEnumerable<IReformedQuery> SortReformedQueries(IEnumerable<IReformedQuery> queries)
            {
                return queries.OrderBy(q => -q.CoOccurrenceCount);
            }
        }


        private class RobinHoodSorter : IReformedQuerySorter
        { 
            public IEnumerable<IReformedQuery> SortReformedQueries(IEnumerable<IReformedQuery> queries)
            {
                queries = queries.ToList();
                var mispellings = SortCorrectionByEditDistance(GetCorrectedQueries(queries)).ToList();
                var others = queries.Except(mispellings).ToList();
                var othersWithSynonym = SortByCoOccurCount(SortBySynonymSimilarity
                    (GetSynonymQueries(others))).ToList();
                var othersWithoutSynonym = SortByCoOccurCount(others.Except(othersWithSynonym)).ToList();
                others.Clear(); 
                others.AddRange(othersWithSynonym);
                others.AddRange(othersWithoutSynonym);
                return MergetList(mispellings, others);
            }

            private IEnumerable<IReformedQuery> SortBySynonymSimilarity(IEnumerable<IReformedQuery>
                queries)
            {
                return queries.OrderByDescending(q => q.ReformedWords.Sum(w => w as SynonymReformedWord != null
                    ? (w as SynonymReformedWord).SynonymSimilarityScore : 0));
            }


            private IEnumerable<IReformedQuery> SortCorrectionByEditDistance(IEnumerable<IReformedQuery> queries )
            {
                return new EditDistanceSorter().SortReformedQueries(queries);
            }

            private static IEnumerable<IReformedQuery> GetCorrectedQueries(IEnumerable<IReformedQuery> queries)
            {
                return queries.Where(q => q.ReformedWords.All(t => t.Category == TermChangeCategory.MISSPELLING ||
                    t.Category == TermChangeCategory.NOT_CHANGED)).ToList();
            }

            private static IEnumerable<IReformedQuery> GetSynonymQueries(IEnumerable<IReformedQuery> queries)
            {
                return queries.Where(q => q.ReformedWords.Any(w => w.Category == TermChangeCategory.GENERAL_SYNONYM ||
                    w.Category == TermChangeCategory.SE_SYNONYM)).ToList();
            }
            
            private static IEnumerable<IReformedQuery> SortByCoOccurCount(IEnumerable<IReformedQuery> queries)
            {
                return queries.OrderBy(q => -q.CoOccurrenceCount);
            }


            private IEnumerable<IReformedQuery> MergetList(List<IReformedQuery> list1, List<IReformedQuery> list2)
            {
                var result = new List<IReformedQuery>();
                var shortList = list1.Count > list2.Count ? list2 : list1;
                var longList = shortList == list1 ? list2 : list1;

                for (var i = 0; i < shortList.Count; i ++)
                {
                    if (shortList == list1)
                    {
                        result.Add(shortList.ElementAt(i));
                        result.Add(longList.ElementAt(i));
                    }
                    else
                    {
                        result.Add(longList.ElementAt(i));
                        result.Add(shortList.ElementAt(i));
                    }
                }
                result.AddRange(longList.GetRange(shortList.Count, longList.Count - shortList.Count));
                return result;
            }
        }


        private class NullSorter : IReformedQuerySorter
        {
            public IEnumerable<IReformedQuery> SortReformedQueries(IEnumerable<IReformedQuery> queries)
            {
                return queries;
            }
        }

        public static IReformedQuerySorter GetReformedQuerySorter(QuerySorterType type)
        {
            if(type == QuerySorterType.EDIT_DISTANCE)
                return new EditDistanceSorter();
            if(type == QuerySorterType.ROBIN_HOOD)
                return new RobinHoodSorter();
            if(type == QuerySorterType.COOCCURRENCE)
                return new CoOccurrenceSorter();
            if(type == QuerySorterType.SCORE)
                return new ScoreBasedSorter();
            return new NullSorter();
        }
    }
}
