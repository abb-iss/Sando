using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Sando.Recommender
{
    /// <summary>
    /// Using this interface to query the dictionary.
    /// </summary>
    public interface IDictionaryQuery
    {
        void StartSelectingWordsAsync(IEnumerable<String> allWords);
    }

    /// <summary>
    /// This is the listener when selected words are ready.
    /// </summary>
    /// <param name="selectedWords"></param>
    public delegate void SelectedWordHandler(IEnumerable<String> selectedWords);


    /// <summary>
    /// All kinds of queries should be created from this factory class.
    /// </summary>
    public class DictionaryQueryFactory
    {
        private abstract class AsyncDictionaryQuery : IDictionaryQuery
        {
            private readonly SelectedWordHandler callBack;
            private IEnumerable<String> selectedWords; 

            protected AsyncDictionaryQuery(SelectedWordHandler callBack)
            {
                this.callBack = callBack;
            }

            public void StartSelectingWordsAsync(IEnumerable<string> allWords)
            {
                var wordsCopy = allWords.ToList();
                var worker = new BackgroundWorker {WorkerReportsProgress = false, WorkerSupportsCancellation = false};
                worker.DoWork += (sender, args) => selectedWords = SearchForWords(wordsCopy);
                worker.RunWorkerCompleted += (sender, args) => callBack.Invoke(selectedWords);
                worker.RunWorkerAsync();
            }

            abstract protected IEnumerable<String> SearchForWords(IEnumerable<String> allWords);
        }

        private class SimilarWordsQuery : AsyncDictionaryQuery
        {
            private const int threashold = 2;
            private readonly String target;
            public SimilarWordsQuery(String target, SelectedWordHandler callBack) : base(callBack)
            {
                this.target = target;
            }

            protected override IEnumerable<string> SearchForWords(IEnumerable<string> allWords)
            {
                return allWords.AsParallel().Where(word => QueryRecommender.Distance(word, target) 
                    < threashold).ToList();
            }
        }

        public static IDictionaryQuery GetSimilarWordsDictionaryQuery(String word, SelectedWordHandler callback)
        {
            return new SimilarWordsQuery(word, callback);
        }
    }
}
