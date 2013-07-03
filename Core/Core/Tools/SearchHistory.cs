using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sando.Core.Tools
{
    public interface ISearchHistoryItem
    {
        string SearchString { get; }
        long TimeStamp { get; }
    }

    public sealed class SearchHistory : IDisposable
    {
        private readonly object locker;
        private readonly List<InternalSearchHistoryItem> allItems;
        private string directory;
        public const string FILE_NAME = "SandoSearchHistory.txt";
        public const int MAXIMUM_COUNT = 1000;

        public SearchHistory()
        {
            locker = new object();
            allItems = new List<InternalSearchHistoryItem>();
        }

        public void Initiatalize(string directory)
        {
            this.directory = directory;
            if (File.Exists(GetFilePath()))
            {
                var lines = File.ReadAllLines(GetFilePath());
                var items = lines.Select(l => new InternalSearchHistoryItem(l));
                lock (locker)
                {
                    allItems.Clear();
                    allItems.AddRange(items);
                }
            }
        }

        private string GetFilePath()
        {
            return Path.Combine(directory, FILE_NAME);
        }

        private class InternalSearchHistoryItem : ISearchHistoryItem, 
            IEquatable<InternalSearchHistoryItem>
        {
            public string SearchString { get; private set; }
            public long TimeStamp { get; private set; }

            internal InternalSearchHistoryItem(string SearchString, long TimeStamp)
            {
                this.SearchString = SearchString;
                this.TimeStamp = TimeStamp;
            }

            internal InternalSearchHistoryItem(string text)
            {
                var timepart = (text.Split().Last());
                this.SearchString = text.Substring(0, text.Count() - timepart.Count() - 1);
                this.TimeStamp = long.Parse(timepart);
            }

            public bool Equals(InternalSearchHistoryItem other)
            {
                return this.SearchString.Equals(other.SearchString);
            }

            public override String ToString()
            {
                return SearchString + " " + TimeStamp;
            }
        }


        public void IssuedSearchString(String query)
        {
            if(String.IsNullOrWhiteSpace(query))
                return;
            var time = DateTime.Now.Ticks;
            var item = new InternalSearchHistoryItem(query, time);
            lock (locker)
            {
                allItems.Remove(item);
                allItems.Add(item);
                if (allItems.Count > MAXIMUM_COUNT)
                {
                    allItems.RemoveAt(0);
                }
            }
        }

        public ISearchHistoryItem[] GetSearchHistoryItems(Predicate<ISearchHistoryItem> selector)
        {
            lock (locker)
            {
                return allItems.Where(selector.Invoke).Cast<ISearchHistoryItem>().OrderByDescending
                    (item => item.TimeStamp).ToArray();
            }
        }

        public void ClearHistory()
        {
            lock (locker)
            {
                allItems.Clear();
            }
        }

        public void Dispose()
        {
            String[] lines;
            lock (locker)
            {
                lines = allItems.Select(item => item.ToString()).ToArray();
                allItems.Clear();
            }
            File.WriteAllLines(GetFilePath(), lines);
            this.directory = null;
        }
    }
}
