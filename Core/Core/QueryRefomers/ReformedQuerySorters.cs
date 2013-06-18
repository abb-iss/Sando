using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.QueryRefomers
{
    internal enum QuerySorterType
    {
        EDIT_DISTANCE,
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
            return new NullSorter();
        }
    }
}
