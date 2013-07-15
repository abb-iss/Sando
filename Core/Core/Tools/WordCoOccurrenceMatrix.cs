using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Sando.Core.QueryRefomers;

namespace Sando.Core.Tools
{
    public interface IWordCoOccurrenceMatrix
    {
        int GetCoOccurrenceCount(String word1, String word2);
        void Initialize(String directory);
        Dictionary<String, int> GetCoOccurredWordsAndCount(String word);
        Dictionary<string, int> GetAllWordsAndCount();
    }

    public class InternalWordCoOccurrenceMatrix : IDisposable, IWordCoOccurrenceMatrix
    {
        private class MatrixEntry : IComparable<MatrixEntry>, IEquatable<MatrixEntry>
        {
            public String Row { get; private set; }
            public String Column { get; private set; }
            public int Count { get; private set; }

            public MatrixEntry(String Row, String Column, int Count)
            {
                this.Row = Row;
                this.Column = Column;
                this.Count = Count;
            }

            public int CompareTo(MatrixEntry other)
            {
                return (this.Row + this.Column).CompareTo(other.Row + other.Column);
            }

            public void IncrementCount()
            {
                this.Count++;
            }

            public void ResetCount()
            {
                this.Count = 1;
            }

            public bool Equals(MatrixEntry other)
            {
                return CompareTo(other) == 0;
            }
        }

        private readonly object locker = new object();
        private List<MatrixEntry> matrix = new List<MatrixEntry>();
        private readonly WorkQueueBasedProcess queue = new WorkQueueBasedProcess();

        private string directory;
        private Action saveAction;

        private const string fileName = "CooccurenceMatrix.txt";

        private const int MAX_WORD_LENGTH = 3;
        private const int MAX_COOCCURRENCE_WORDS_COUNT = 100;
        private const int GRAM_NUMBER = 3;
        private const int SAVE_EVERY_MINUTES = 10;

        public void Initialize(String directory)
        {
            lock (locker)
            {
                matrix.Clear();
                this.directory = directory;
                ReadMatrixFromFile();

                saveAction = new Action(WriteMatrixToFile);
                TimedProcessor.GetInstance().AddTimedTask(saveAction, 
                    SAVE_EVERY_MINUTES * 60 * 1000);
            }
        }

        public Dictionary<string, int> GetCoOccurredWordsAndCount(string word)
        {
            lock (locker)
            {
                var columns = new Dictionary<String, int>();
                foreach (var entry in matrix)
                {
                    if (entry.Column.CompareTo(word) == 0 || entry.Row.CompareTo(word) == 0)
                    {
                        var key = entry.Column.CompareTo(word) == 0 ? entry.Row : entry.Column;
                        columns.Add(key, entry.Count);
                    }
                }
                return columns;
                /*
                int start = ~matrix.BinarySearch(CreateEntry(word, ""));
                if (start < 0 || start >= matrix.Count)
                    return columns;

                for (; !matrix.ElementAt(start).Row.Equals(word); start ++);
                
                for (int i = start; i < matrix.Count; i++)
                {
                    var entry = matrix.ElementAt(i);
                    if (!entry.Row.Equals(word))
                    {
                        break;
                    }
                    columns.Add(entry.Column, entry.Count);
                }
                return columns;*/
            }
        }

        private void ReadMatrixFromFile()
        {
            if (File.Exists(GetMatrixFilePath()))
            {
                var allLines = File.ReadAllLines(GetMatrixFilePath());
                foreach (string line in allLines)
                {
                    var parts = line.Split();
                    var row = parts[0];
                    for (int i = 1; i < parts.Count(); i++)
                    {
                        var splits = parts[i].Split(':');
                        var column = splits[0];
                        var count = Int32.Parse(splits[1]);
                        matrix.Add(CreateEntry(row, column, count));
                    }
                }
                matrix = matrix.OrderBy(m => m).ToList();
            }
        }

        private string GetMatrixFilePath()
        {
            return Path.Combine(directory, fileName);
        }

        private void WriteMatrixToFile()
        {
            var sb = new StringBuilder();
            var groups = matrix.GroupBy(p => p.Row);
            foreach (var group in groups)
            {
                var row = group.First().Row;
                var line = row + ' ' + group.Select(g => g.Column + ":" + g.Count).
                    Aggregate((s1, s2) => s1 + ' ' + s2);
                sb.AppendLine(line);
            }
            File.WriteAllText(GetMatrixFilePath(), sb.ToString());
        }


        private String Preprocess(String word)
        {
            return word.ToLower().Trim();
        }

        public int GetCoOccurrenceCount(String word1, String word2)
        {
            lock (locker)
            {
                var target = CreateEntry(word1, word2);
                int index = matrix.BinarySearch(target);
                return index >= 0 ? matrix.ElementAt(index).Count : 0;
            }
        }
 

        public void HandleCoOcurrentWordsSync(IEnumerable<String> words)
        {
            AddMatrixEntriesSync(words);
        }

        public void HandleCoOcurrentWordsAsync(IEnumerable<String> words)
        {
            queue.Enqueue(AddMatrixEntriesSync, words);
        }

        private IEnumerable<MatrixEntry> GetEntries(IEnumerable<string> words)
        {
            var list = LimitWordNumber(FilterOutBadWords(words).
                                           Distinct().ToList()).ToList();
            var allEntries = new List<MatrixEntry>();
            for (int i = 0; i < list.Count; i++)
            {
                var word1 = list.ElementAt(i);
                for (int j = i; j < list.Count; j++)
                {
                    var word2 = list.ElementAt(j);
                    allEntries.Add(CreateEntry(word1, word2));
                }
            }
            return allEntries;
        }

        private IEnumerable<MatrixEntry> GetBigramEntries(IEnumerable<string> words)
        {
            var list = words.ToList();
            var allEntries = new List<MatrixEntry>();
            int i;
            for (i = 0; i + GRAM_NUMBER - 1 < list.Count; i++)
            {
                allEntries.AddRange(GetEntries(list.GetRange(i, GRAM_NUMBER)));
            }

            // Check if having leftovers.
            if (i + GRAM_NUMBER - 1 != list.Count - 1 && list.Any())
            {
                allEntries.AddRange(GetEntries(list.GetRange(i, list.Count - i)));
            }
            
            return allEntries;
        }


        private IEnumerable<String> LimitWordNumber(List<string> words)
        {
            return (words.Count > MAX_COOCCURRENCE_WORDS_COUNT)
                       ? words.GetRange(0, MAX_COOCCURRENCE_WORDS_COUNT)
                       : words;
        }


        private int AddMatrixEntriesSync(IEnumerable<String> words)
        {
            var allEntries = GetBigramEntries(words);
            lock (locker)
            {
                foreach (var target in allEntries)
                {
                    int end = matrix.BinarySearch(target);
                    if (end >= 0)
                    {
                        matrix.ElementAt(end).IncrementCount();
                    }
                    else
                    {
                        target.ResetCount();
                        int index = ~end;
                        matrix.Insert(index, target);
                    }
                }
            }
            return 0;
        }

        private IEnumerable<String> FilterOutBadWords(IEnumerable<String> words)
        {
            return words.Where(w => w.Length >= MAX_WORD_LENGTH
                || w.Contains(' ') || w.Contains(':'));
        }


        private MatrixEntry CreateEntry(String word1, String word2, int count = 1)
        {
            word1 = Preprocess(word1);
            word2 = Preprocess(word2);
            if (word1.CompareTo(word2) > 0)
            {
                var temp = word2;
                word2 = word1;
                word1 = temp;
            }
            return new MatrixEntry(word1, word2, count);
        }

        public void Dispose()
        {
            lock (locker)
            {
                WriteMatrixToFile();
                matrix.Clear();
                TimedProcessor.GetInstance().RemoveTimedTask(saveAction);
                directory = null;
            }
        }

        public Dictionary<string, int> GetAllWordsAndCount()
        {
            lock (locker)
            {
                return matrix.Where(entry => entry.Column.Equals(entry.Row)).
                    ToDictionary(entry => entry.Row, entry => entry.Count);
            }
        }
    }
}
