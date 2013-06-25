using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.Core.Tools;

namespace Sando.Core.QueryRefomers
{
    public class ReformedQueryBuilder
    {
        private readonly List<List<ReformedWord>> reformedTermLists = new
            List<List<ReformedWord>>();

        private readonly List<Predicate<IReformedQuery>> QueryFilters = new 
            List<Predicate<IReformedQuery>>();

        private readonly IWordCoOccurrenceMatrix coOccurrenceMatrix;


        public ReformedQueryBuilder(IWordCoOccurrenceMatrix coOccurrenceMatrix)
        {
            this.coOccurrenceMatrix = coOccurrenceMatrix;
            QueryFilters.Add(IsEveryWordPairExisting);
        }


        private Boolean IsEveryWordPairExisting(IReformedQuery query)
        {
            var words = query.ReformedQuery.Select(q => q.NewTerm).ToList();
            for (int i = 0; i < words.Count - 1; i ++)
            {
                for (int j = i + 1; j < words.Count; j ++)
                {
                    if (coOccurrenceMatrix.GetCoOccurrenceCount(words.ElementAt(i),
                        words.ElementAt(j)) == 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private class InternalReformedQuery : IReformedQuery, ICloneable
        {
            private readonly List<ReformedWord> allTerms;

            public InternalReformedQuery()
            {
                this.allTerms = new List<ReformedWord>();
            }

            private InternalReformedQuery(IEnumerable<ReformedWord> allTerms)
            {
                this.allTerms = allTerms.ToList();
            }

            public IEnumerable<ReformedWord> ReformedQuery
            {
                get { return allTerms; }
            }

            public IEnumerable<string> ReformedTerms {
                get { return allTerms.Select(t => t.NewTerm).ToList(); }
            }

            public string ReformExplanation {
                get
                {
                    var sb = new StringBuilder();
                    foreach (ReformedWord reformedTerm in allTerms)
                    {
                        if (reformedTerm.Category != TermChangeCategory.NOT_CHANGED)
                        {
                            sb.Append(reformedTerm.ReformExplanation + ";");
                        }
                    }
                    return sb.ToString();
                }
            }

            public string QueryString {
                get { 
                    var sb = new StringBuilder();
                    foreach (ReformedWord term in allTerms)
                    {
                        sb.Append(term.NewTerm);
                        sb.Append(" ");
                    }
                    return sb.ToString().Trim();
                }
            }

            public object Clone()
            {
                return new InternalReformedQuery(allTerms);
            }

            public void AppendTerm(ReformedWord term)
            {
                allTerms.Add(term);
            }

            public bool Equals(IReformedQuery other)
            {
                return this.QueryString.Equals(other.QueryString);
            }

            public InternalReformedQuery RemoveRedundantTerm()
            {
                var list = allTerms.Distinct(new NewTermEqualityComparer()).ToList();
                allTerms.Clear(); 
                allTerms.AddRange(list);
                return this;
            }

            private class NewTermEqualityComparer : IEqualityComparer<ReformedWord>
            {
                public bool Equals(ReformedWord x, ReformedWord y)
                {
                    return x.NewTerm.Equals(y.NewTerm) || x.NewTerm.GetStemmedQuery().
                        Equals(y.NewTerm.GetStemmedQuery());
                }

                public int GetHashCode(ReformedWord obj)
                {
                    return 0;
                }
            }
        }


        public void StartBuilding()
        {
            this.reformedTermLists.Clear();
        }

        public void AddReformedTerms(IEnumerable<ReformedWord> newTerms)
        {
            this.reformedTermLists.Add(newTerms.ToList());
        }

        public IEnumerable<IReformedQuery> GetAllPossibleReformedQueriesSoFar()
        {
            var allReformedQuries = new List<InternalReformedQuery> {new InternalReformedQuery()};
            var allQuries = reformedTermLists.Aggregate(allReformedQuries, 
                (current, reformedTermList) => current.SelectMany(q => 
                    GenerateNewQueriesByAppendingTerms(q, reformedTermList)).ToList()).ToList();
            // too strict?
            // return RemoveDuplication(FilterOutBadQueries(allQuries.Select(q => q.RemoveRedundantTerm())));
            return RemoveDuplication(allQuries.Select(q => q.RemoveRedundantTerm()));
        }

        private IEnumerable<IReformedQuery> FilterOutBadQueries(IEnumerable<InternalReformedQuery> allQuries)
        {
            return allQuries.Where(q => QueryFilters.All(f => f.Invoke(q))).ToList();
        }

        private IEnumerable<IReformedQuery> RemoveDuplication(IEnumerable<IReformedQuery> queries)
        {
            return queries.RemoveRedundance();
        }

        private IEnumerable<InternalReformedQuery> GenerateNewQueriesByAppendingTerms(InternalReformedQuery query, 
            ICollection<ReformedWord> newTerms)
        {
            var list = new List<InternalReformedQuery>();
            for (var i = 0; i < newTerms.Count; i++)
            {
                var extendedQuery = (InternalReformedQuery) query.Clone();
                extendedQuery.AppendTerm(newTerms.ElementAt(i));
                list.Add(extendedQuery);
            }
            return list;
        }
    }
}
