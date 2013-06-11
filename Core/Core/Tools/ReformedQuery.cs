using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.Tools
{
    public enum QuryRecommendationLevel
    {
        REPLACE,
    }

    public enum TermChangeCategory
    {
        NOT_CHANGED,
        MISSPELLING_CORRECTION,
        SYNONYM_IN_SE_THESAURUS,
    }

    public interface IReformedQuery
    {
        QuryRecommendationLevel QuryRecommendationLevel { get; }
        IEnumerable<IReformedTerm> ReformedQuery { get; }
    }

    public interface IReformedTerm
    {
        TermChangeCategory Category { get; }
        String OriginalTerm { get; }
        String ReformedTerm { get; }
    }

    public class ReformedQueryBuilder
    {
        private readonly List<List<IReformedTerm>> reformedTermLists = new
            List<List<IReformedTerm>>();

        private class InternalReformedQuery : IReformedQuery, ICloneable
        {
            private readonly List<IReformedTerm> allTerms;

            public InternalReformedQuery()
            {
                this.allTerms = new List<IReformedTerm>();
            }

            private InternalReformedQuery(IEnumerable<IReformedTerm> allTerms)
            {
                this.allTerms = allTerms.ToList();
            }

            public QuryRecommendationLevel QuryRecommendationLevel { get; private set; }

            public IEnumerable<IReformedTerm> ReformedQuery
            {
                get { return allTerms; }
            }

            public object Clone()
            {
                return new InternalReformedQuery(allTerms);
            }

            public void AppendTerm(IReformedTerm term)
            {
                allTerms.Add(term);
            }
        }


        public void StartBuilding()
        {
            this.reformedTermLists.Clear();
        }

        public void AddReformedTerms(IEnumerable<IReformedTerm> newTerms)
        {
            this.reformedTermLists.Add(newTerms.ToList());
        }

        public IEnumerable<IReformedQuery> GetAllPossibleReformedQueriesSoFar()
        {
            var allReformedQuries = new List<InternalReformedQuery> {new InternalReformedQuery()};
            return reformedTermLists.Aggregate(allReformedQuries, 
                (current, reformedTermList) => current.SelectMany(q => 
                    GenerateNewQueriesByAppendingTerms(q, reformedTermList)).ToList());
        }

        private IEnumerable<InternalReformedQuery> GenerateNewQueriesByAppendingTerms(InternalReformedQuery query, 
            ICollection<IReformedTerm> newTerms)
        {
            var list = new List<InternalReformedQuery>();
            for (int i = 0; i < newTerms.Count; i++)
            {
                var extendedQuery = (InternalReformedQuery) query.Clone();
                extendedQuery.AppendTerm(newTerms.ElementAt(i));
                list.Add(extendedQuery);
            }
            return list;
        }
    }
}
