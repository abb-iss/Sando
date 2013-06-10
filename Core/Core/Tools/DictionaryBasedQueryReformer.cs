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
    public delegate void ImprovedTermsReady(IEnumerable<IEnumerable<String>> improvedTerms);

    public class DictionaryBasedQueryReformer
    {
        const int SIMILAR_WORDS_MAX_COUNT = 2;
        const int SYNONYMS_MAX_COUNT = 2;

        private readonly DictionaryBasedSplitter dictionary;
        private readonly IThesaurus seThesaurus;

        public DictionaryBasedQueryReformer(DictionaryBasedSplitter dictionary)
        {
            this.dictionary = dictionary;
            this.seThesaurus = SESpecificThesaurus.GetInstance();
        }

        public void Initialize()
        {
            ((SESpecificThesaurus)seThesaurus).Initialize();
        }

        public void ReformTermsAsynchronously(IEnumerable<String> terms, ImprovedTermsReady callback)
        {
            var worker = new BackgroundWorker { WorkerReportsProgress = false, 
                WorkerSupportsCancellation = false };
            worker.DoWork += (sender, args) => callback.Invoke(ReformTermsSynchronously(terms));
            worker.RunWorkerAsync();
        }

        public IEnumerable<IEnumerable<string>> ReformTermsSynchronously(IEnumerable<string> terms)
        {
            var termList = terms.ToList();
            if (termList.Any())
            {
                var results = new List<IEnumerable<String>>();
                var betterFirstTerms = FindBetterTerms(termList.First()).ToList();
                var restTermLists = ReformTermsSynchronously(termList.GetRange(1, termList.Count() 
                    - 1)).ToList();           
                foreach (String betterFirstTerm in betterFirstTerms)
                {
                    var list = new List<String> {betterFirstTerm};
                    if (restTermLists.Any())
                    {
                        foreach (var restTermList in restTermLists)
                        {
                            var copyList = list.ToList();
                            copyList.AddRange(restTermList);
                            results.Add(copyList);
                        }
                    }
                    else
                    {
                        results.Add(list);
                    }
                }
                return results;
            }
            return Enumerable.Empty<IEnumerable<String>>();
        }

        private IEnumerable<String> FindBetterTerms(String word)
        {
            if (!dictionary.DoesWordExist(word))
            {
                var list = new List<String>();
                list.AddRange(FindShapeSimilarWordsInLocalDictionary(word));
                list.AddRange(FindSynonymsInLocalDictionary(word));
                return list;
            }
            return new []{word};
        }

        private IEnumerable<String> FindShapeSimilarWordsInLocalDictionary(String word)
        {
            var list = dictionary.FindSimilarWords(word).ToList();
            if (list.Count >= SIMILAR_WORDS_MAX_COUNT)
                list = list.GetRange(0, SIMILAR_WORDS_MAX_COUNT);
            return list;
        }

        private IEnumerable<String> FindSynonymsInLocalDictionary(String word)
        {
            var synonyms = seThesaurus.GetSynonyms(word);
            var list = synonyms.Where(w => dictionary.DoesWordExist(w)).ToList();
            if (list.Count() >= SYNONYMS_MAX_COUNT)
                list = list.GetRange(0, SYNONYMS_MAX_COUNT);
            return list;
        }
    }
}
