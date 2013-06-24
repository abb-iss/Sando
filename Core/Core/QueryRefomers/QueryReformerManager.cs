using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Sando.Core.Tools;

namespace Sando.Core.QueryRefomers
{
    /// <summary>
    /// This is the listener when improved terms are ready.
    /// </summary>
    /// <param name="improvedTerms"></param>
    public delegate void ImprovedQueryReady(IEnumerable<IReformedQuery> improvedTerms);

    public class QueryReformerManager
    {
       
        private readonly DictionaryBasedSplitter dictionary;
      
        public QueryReformerManager(DictionaryBasedSplitter dictionary)
        {
            this.dictionary = dictionary;
        }

        public void Initialize()
        {
            SeSpecificThesaurus.GetInstance().Initialize();
            GeneralEnglishThesaurus.GetInstance().Initialize();
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
                
                var builder = new ReformedQueryBuilder(dictionary);
                foreach (string term in termList)
                {
                    var neigbors = GetNeighbors(termList, term);
                    builder.AddReformedTerms(FindBetterTerms(term, neigbors));
                }
                return TrimExcessiveRecommendations(GetReformedQuerySorter().SortReformedQueries
                    (builder.GetAllPossibleReformedQueriesSoFar()));
            }
            return Enumerable.Empty<IReformedQuery>();
        }

        private IEnumerable<IReformedQuery> TrimExcessiveRecommendations(IEnumerable<IReformedQuery> queries)
        {
            var list = queries.ToList();
            return list.Count() > QuerySuggestionConfigurations.MAXIMUM_RECOMMENDATIONS_COUNT
                ? list.GetRange(0, QuerySuggestionConfigurations.MAXIMUM_RECOMMENDATIONS_COUNT) : list;
        }

        private IReformedQuerySorter GetReformedQuerySorter()
        {
            return ReformedQuerySorters.GetReformedQuerySorter(QuerySorterType.EDIT_DISTANCE);
        }

        private IEnumerable<ReformedWord> FindBetterTerms(string word, IEnumerable<string> neigbors)
        {
            if (!dictionary.DoesWordExist(word, DictionaryOption.IncludingStemming) 
                && !IsWordQuoted(word))
            {
                var list = new List<ReformedWord>();
                list.AddRange(FindShapeSimilarWordsInLocalDictionary(word));
                list.AddRange(FindSynonymsInDictionaries(word));
                list.AddRange(FindCoOccurredTerms(word, neigbors));
                return list;
            }
            return new []{new ReformedWord(TermChangeCategory.NOT_CHANGED, 
                word, word, String.Empty)};
        }

        private bool IsWordQuoted(string word)
        {
            word = word.Trim();
            return word.StartsWith("\"") && word.EndsWith("\"");
        }

        private IEnumerable<ReformedWord> FindShapeSimilarWordsInLocalDictionary(String word)
        {
            var reformer = new TypoCorrectionReformer(dictionary);
            return reformer.GetReformedTarget(word).ToList();
        }

        private IEnumerable<ReformedWord> FindSynonymsInDictionaries(String word)
        {
            var reformer = new SeThesaurusWordReformer(dictionary);
            var list = reformer.GetReformedTarget(word).ToList();
            list.AddRange(new GeneralThesaurusWordReformer(dictionary).GetReformedTarget(word));
            return list;
        }

        private IEnumerable<String> GetNeighbors(IEnumerable<String> words, String target)
        {
            return words.Where(w => !w.Equals(target));
        }
        
        private IEnumerable<ReformedWord> FindCoOccurredTerms(String word, IEnumerable<String> neighbors)
        {
            var reformer = new CoOccurrenceBasedReformer(dictionary);
            reformer.SetContextWords(neighbors);
            return reformer.GetReformedTarget(word);
        }

   
    }
}
