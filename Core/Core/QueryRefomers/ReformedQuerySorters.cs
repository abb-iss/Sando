using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.QueryRefomers
{
    internal enum QuerySorterType
    {
        EDIT_DISTANCE,
        ROBIN_HOOD,
        NULL_SORTER,
    }


    internal class ReformedQuerySorters
    {
        private class EditDistanceSorter : IReformedQuerySorter
        {
            public IEnumerable<IReformedQuery> SortReformedQueries(IEnumerable<IReformedQuery> queries)
            {
                return queries.OrderBy(GetTotalDistance);
            }

            private int GetTotalDistance(IReformedQuery query)
            {
                return query.ReformedQuery.Sum(term => term.DistanceFromOriginal);
            }
        }

        private class RobinHoodSorter : IReformedQuerySorter
        { 
            public IEnumerable<IReformedQuery> SortReformedQueries(IEnumerable<IReformedQuery> queries)
            {
                queries = queries.ToList();
                var mispellings =
                    queries.Where(q => q.ReformedQuery.All(t => t.Category == TermChangeCategory.MISSPELLING ||
                        t.Category == TermChangeCategory.NOT_CHANGED)).ToList();
                var others = queries.Except(mispellings).ToList();
                mispellings = new EditDistanceSorter().SortReformedQueries(mispellings).ToList();
                others = new EditDistanceSorter().SortReformedQueries(others).ToList();
                return MergetList(mispellings, others);
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
            return new NullSorter();
        }
    }
}
