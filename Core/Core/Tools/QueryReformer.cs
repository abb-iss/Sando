using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Sando.Core.Tools
{
    /// <summary>
    /// This is the listener when improved terms are ready.
    /// </summary>
    /// <param name="improvedTerms"></param>
    public delegate void ImprovedQueryReady(IEnumerable<IReformedQuery> improvedTerms);

    public class QueryReformer
    {
        const int SIMILAR_WORDS_MAX_COUNT = 2;
        const int SYNONYMS_MAX_COUNT = 2;

        private readonly DictionaryBasedSplitter dictionary;
        private readonly IThesaurus seThesaurus;

        public QueryReformer(DictionaryBasedSplitter dictionary)
        {
            this.dictionary = dictionary;
            this.seThesaurus = SESpecificThesaurus.GetInstance();
        }

        public void Initialize()
        {
            ((SESpecificThesaurus)seThesaurus).Initialize();
        }

        public void ReformTermsAsynchronously(IEnumerable<String> terms, ImprovedQueryReady callback)
        {
            var worker = new BackgroundWorker { WorkerReportsProgress = false, 
                WorkerSupportsCancellation = false };
            worker.DoWork += (sender, args) => callback.Invoke(ReformTermsSynchronously(terms));
            worker.RunWorkerAsync();
        }

        public IEnumerable<IReformedQuery> ReformTermsSynchronously(IEnumerable<string> terms)
        {
            var termList = terms.ToList();
            if (termList.Any())
            {
                var builder = new ReformedQueryBuilder();
                foreach (string term in termList)
                {
                    builder.AddReformedTerms(FindBetterTerms(term));
                }
                return builder.GetAllPossibleReformedQueriesSoFar();
            }
            return Enumerable.Empty<IReformedQuery>();
        }

        private IEnumerable<IReformedTerm> FindBetterTerms(String word)
        {
            if (!dictionary.DoesWordExist(word))
            {
                var list = new List<IReformedTerm>();
                list.AddRange(FindShapeSimilarWordsInLocalDictionary(word));
                list.AddRange(FindSynonymsInLocalDictionary(word));
                return list;
            }
            return new []{new InternalReformedTerm(TermChangeCategory.NOT_CHANGED, 
                word, word)};
        }

        private IEnumerable<IReformedTerm> FindShapeSimilarWordsInLocalDictionary(String word)
        {
            var list = dictionary.FindSimilarWords(word).Select(w => new InternalReformedTerm
                (TermChangeCategory.MISSPELLING_CORRECTION, word, w)).ToList();
            if (list.Count >= SIMILAR_WORDS_MAX_COUNT)
                list = list.GetRange(0, SIMILAR_WORDS_MAX_COUNT);
            return list;
        }

        private IEnumerable<IReformedTerm> FindSynonymsInLocalDictionary(String word)
        {
            var synonyms = seThesaurus.GetSynonyms(word).Select(w => new InternalReformedTerm
                (TermChangeCategory.SYNONYM_IN_SE_THESAURUS, word, w));
            var list = synonyms.Where(w => dictionary.DoesWordExist(w.ReformedTerm)).ToList();
            if (list.Count() >= SYNONYMS_MAX_COUNT)
                list = list.GetRange(0, SYNONYMS_MAX_COUNT);
            return list;
        }

        private class InternalReformedTerm : IReformedTerm 
        {
            public TermChangeCategory Category { get; private set; }
            public string OriginalTerm { get; private set; }
            public string ReformedTerm { get; private set; }

            public InternalReformedTerm(TermChangeCategory category, String originalTerm, 
                String reformedTerm)
            {
                this.Category = category;
                this.OriginalTerm = originalTerm;
                this.ReformedTerm = reformedTerm;
            }
        }
    }
}
