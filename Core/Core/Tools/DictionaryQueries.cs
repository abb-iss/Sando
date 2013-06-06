using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sando.DependencyInjection;

namespace Sando.Core.Tools
{
    /// <summary>
    /// This is the listener when selected words are ready.
    /// </summary>
    /// <param name="selectedWords"></param>
    public delegate void SelectedWordsHandler(IEnumerable<String> selectedWords);

    /// <summary>
    /// All kinds of queries should be created from this factory class.
    /// </summary>
    public class DictionaryAsyncQueries
    {
        private readonly DictionaryBasedSplitter dictionary;

        public DictionaryAsyncQueries(DictionaryBasedSplitter dictionary)
        {
            this.dictionary = dictionary;
        }

        public void FindSimilarWords(String word, SelectedWordsHandler callback)
        {
            var worker = new BackgroundWorker {WorkerReportsProgress = false, WorkerSupportsCancellation = false};
            worker.DoWork += (sender, args) => callback.Invoke(dictionary.FindSimilarWords(word));
            worker.RunWorkerAsync();
        }

        public void FindSynonyms(String word, SelectedWordsHandler callback)
        {
            
        }
    }
}
