using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.Tools
{
    public enum QuryReformLevel
    {
        NOT_REFORMED,
        REPLACING,
        PURE_RECOMMENDATION,
        UNSET,
    }

    public enum TermChangeCategory
    {
        NOT_CHANGED,
        MISSPELLING_CORRECTION,
        SYNONYM_IN_SE_THESAURUS,
    }

    public interface IReformedQuery
    {
        QuryReformLevel QuryReformLevel { get; }
        IEnumerable<IReformedTerm> ReformedQuery { get; }
        IEnumerable<String> GetReformedTerms { get; }
        String ReformExplanation { get; }
    }

    public interface IReformedTerm
    {
        TermChangeCategory Category { get; }
        String OriginalTerm { get; }
        String ReformedTerm { get; }
        String ReformExplanation { get; }
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
                this.QuryReformLevel = QuryReformLevel.UNSET;
            }

            private InternalReformedQuery(IEnumerable<IReformedTerm> allTerms)
            {
                this.allTerms = allTerms.ToList();
                this.QuryReformLevel = QuryReformLevel.UNSET;
            }

            public QuryReformLevel QuryReformLevel { get; set; }

            public IEnumerable<IReformedTerm> ReformedQuery
            {
                get { return allTerms; }
            }

            public IEnumerable<string> GetReformedTerms {
                get { return allTerms.Select(t => t.ReformedTerm).ToList(); }
            }

            public string ReformExplanation {
                get
                {
                    var sb = new StringBuilder();
                    foreach (IReformedTerm reformedTerm in allTerms)
                    {
                        if (reformedTerm.Category != TermChangeCategory.NOT_CHANGED)
                        {
                            sb.Append(reformedTerm.ReformExplanation + ";");
                        }
                    }
                    return sb.ToString();
                }
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
            var allQuries =  reformedTermLists.Aggregate(allReformedQuries, 
                (current, reformedTermList) => current.SelectMany(q => 
                    GenerateNewQueriesByAppendingTerms(q, reformedTermList)).ToList());
            TagReformedQueries(allQuries);
            return allQuries;
        }

        private void TagReformedQueries(IEnumerable<IReformedQuery> queries)
        {
            queries = queries.ToList();
            new NonReformedQueryTagger().Tag(queries.Cast<InternalReformedQuery>());
            new ReplacingQueryTagger().Tag(queries.Cast<InternalReformedQuery>());
            new PureRecommendedQueryTagger().Tag(queries.Cast<InternalReformedQuery>());
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

        private abstract class QueryTagger
        {
            protected abstract bool IsQuerySelected(InternalReformedQuery query);
            protected abstract QuryReformLevel GetTag();

            public void Tag(IEnumerable<InternalReformedQuery> allQueries)
            {
                var queries = allQueries.Where(IsQuerySelected);
                foreach (InternalReformedQuery q in queries)
                {
                    q.QuryReformLevel = GetTag();
                }
            }
        }

        private sealed class NonReformedQueryTagger : QueryTagger
        {
            protected override bool IsQuerySelected(InternalReformedQuery query)
            {
                return query.ReformedQuery.All(t => t.Category == TermChangeCategory.NOT_CHANGED);
            }

            protected override QuryReformLevel GetTag()
            {
                return QuryReformLevel.NOT_REFORMED;
            }
        }

        private sealed class ReplacingQueryTagger : QueryTagger
        {
            protected override bool IsQuerySelected(InternalReformedQuery query)
            {
                var terms = query.ReformedQuery.ToList();
                return terms.Any(t => t.Category == TermChangeCategory.MISSPELLING_CORRECTION) &&
                       terms.All(t => t.Category != TermChangeCategory.SYNONYM_IN_SE_THESAURUS);
            }

            protected override QuryReformLevel GetTag()
            {
                return QuryReformLevel.REPLACING;
            }
        }

        private sealed class PureRecommendedQueryTagger : QueryTagger
        {
            protected override bool IsQuerySelected(InternalReformedQuery query)
            {
                var terms = query.ReformedQuery.ToList();
                return terms.Any(t => t.Category == TermChangeCategory.SYNONYM_IN_SE_THESAURUS) &&
                       terms.All(t => t.Category != TermChangeCategory.MISSPELLING_CORRECTION);
            }

            protected override QuryReformLevel GetTag()
            {
                return QuryReformLevel.PURE_RECOMMENDATION;
            }
        }
    }
}
