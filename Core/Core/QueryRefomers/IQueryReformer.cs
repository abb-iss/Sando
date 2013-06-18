using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.Core.Tools;

namespace Sando.Core.QueryRefomers
{
    public interface IQueryReformer
    {
        void SetOriginalQuery(IEnumerable<String> words);
        void Reform();
        IEnumerable<IReformedQuery> GetReformedQueries();
    }

    public interface IReformedQuerySorter
    {
        IEnumerable<IReformedQuery> SortReformedQueries(IEnumerable<IReformedQuery> queries);
    }

    internal abstract class AbstractWordReformer
    {
        protected readonly DictionaryBasedSplitter localDictionary;

        protected AbstractWordReformer(DictionaryBasedSplitter localDictionary)
        {
            this.localDictionary = localDictionary;
        }

        public IEnumerable<ReformedWord> GetReformedTarget(String target)
        {
            return GetReformedTargetInternal(target).Where(w => this.localDictionary.
                DoesWordExist(w.NewTerm, DictionaryOption.IncludingStemming));
        }

        protected abstract IEnumerable<ReformedWord> GetReformedTargetInternal(String target);
    }

    internal abstract class AbstractContextSensitiveWordReformer : AbstractWordReformer
    {
        protected AbstractContextSensitiveWordReformer(DictionaryBasedSplitter localDictionary) 
            : base(localDictionary)
        {
        }

        public abstract void SetWordsBeforeTarget(IEnumerable<String> words);
        public abstract void SetWordsAfterTarget(IEnumerable<String> words);
    }

    public enum TermChangeCategory
    {
        NOT_CHANGED,
        MISSPELLING,
        SE_SYNONYM,
        GENERAL_SYNONYM,
    }

    public interface IReformedQuery
    {
        IEnumerable<ReformedWord> ReformedQuery { get; }
        IEnumerable<String> GetReformedTerms { get; }
        String ReformExplanation { get; }
    }


    public class ReformedWord
    {
        public TermChangeCategory Category { get; private set; }
        public string OriginalTerm { get; private set; }
        public string NewTerm { get; private set; }
        public int DistanceFromOriginal { get; private set; }
        public string ReformExplanation { get; private set; }

        public ReformedWord(TermChangeCategory category, String originalTerm,
            String newTerm, String reformExplanation)
        {
            this.Category = category;
            this.OriginalTerm = originalTerm;
            this.NewTerm = newTerm;
            this.ReformExplanation = reformExplanation;
            this.DistanceFromOriginal = new Levenshtein().LD(this.OriginalTerm,
                this.NewTerm);
        }
    }
}
