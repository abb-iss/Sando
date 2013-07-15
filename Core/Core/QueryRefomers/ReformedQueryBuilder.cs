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
            var words = query.ReformedWords.Select(q => q.NewTerm).ToList();
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
            private readonly IWordCoOccurrenceMatrix matrix;

            public InternalReformedQuery(IWordCoOccurrenceMatrix matrix)
            {
                this.allTerms = new List<ReformedWord>();
                this.matrix = matrix;
            }

            private InternalReformedQuery(IEnumerable<ReformedWord> allTerms, IWordCoOccurrenceMatrix matrix)
            {
                this.allTerms = allTerms.ToList();
                this.matrix = matrix;
            }

            public IEnumerable<ReformedWord> ReformedWords
            {
                get { return allTerms; }
            }

            public IEnumerable<string> WordsAfterReform {
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

            public int CoOccurrenceCount { 
                get
                {
                    int count = 0;
                    int length = allTerms.Count;
                    for (var i = 0; i < length - 1; i ++)
                    {
                        var word1 = allTerms.ElementAt(i).NewTerm;
                        var left = allTerms.GetRange(i + 1, length - i - 1).Select(t => t.NewTerm);
                        count += left.Where(word2 => !word1.Equals(word2)).Sum
                            (word2 => matrix.GetCoOccurrenceCount(word1, word2));
                    }
                    return count;
                }
            }

            public int EditDistance {
                get { return allTerms.Sum(t => t.DistanceFromOriginal); }
            }

            public object Clone()
            {
                return new InternalReformedQuery(allTerms, matrix);
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

            public InternalReformedQuery RemoveShortTerms()
            {
                var list = allTerms.ToList();
                for (int i = 0; i < allTerms.Count; i++)
                {
                    if (allTerms.ElementAt(i).NewTerm.Length < 2)
                    {
                        list.RemoveAt(i);
                    }
                }
                allTerms.Clear();
                allTerms.AddRange(list);
                return this;
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
            var allReformedQuries = new List<InternalReformedQuery> {new 
                InternalReformedQuery(this.coOccurrenceMatrix)};
            var allQuries = reformedTermLists.Aggregate(allReformedQuries, 
                (current, reformedTermList) => current.SelectMany(q => 
                    GenerateNewQueriesByAppendingTerms(q, reformedTermList)).ToList()).ToList();
            // too strict?
            // return RemoveDuplication(FilterOutBadQueries(allQuries.Select(q => q.RemoveRedundantTerm())));
            return RemoveDuplication(allQuries.Select(q => q.RemoveRedundantTerm().RemoveShortTerms()));
        }

        private IEnumerable<IReformedQuery> FilterOutBadQueries(IEnumerable<InternalReformedQuery> allQuries)
        {
            return allQuries.Where(q => QueryFilters.All(f => f.Invoke(q))).ToList();
        }

        private IEnumerable<IReformedQuery> RemoveDuplication(IEnumerable<IReformedQuery> queries)
        {
            queries = queries.ToArray();
            var queryList = queries.ToList();
            var stemmedNewQueries = queries.Select(GetStemmedNewQuery).ToList();
            for (int i = stemmedNewQueries.Count() - 1; i >= 0; i--)
            {
                var current = stemmedNewQueries.ElementAt(i);
                var before = stemmedNewQueries.GetRange(0, i);
                if(before.Contains(current))
                    queryList.RemoveAt(i);
            }
            return queryList;
        }

        private string GetStemmedNewQuery(IReformedQuery query)
        {
            var sb = new StringBuilder();
            foreach (var word in query.ReformedWords.Select(w => w.NewTerm))
            {
                sb.Append(word.GetStemmedQuery());
                sb.Append(" ");
            }
            return sb.ToString().Trim();
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
