using System;
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
    }

    public class InternalWordCoOccurrenceMatrix : IDisposable, IWordCoOccurrenceMatrix
    {
        private class MatrixEntry : IComparable<MatrixEntry>, IEquatable<MatrixEntry>
        {
            public String Row { get; private set; }
            public String Column { get; private set; }
            
            public MatrixEntry(String Row, String Column)
            {
                this.Row = Row;
                this.Column = Column;
            }

            public int CompareTo(MatrixEntry other)
            {
                return (this.Row + this.Column).CompareTo(other.Row + other.Column);
            }

            public bool Equals(MatrixEntry other)
            {
                return CompareTo(other) == 0;
            }

            public String ToString()
            {
                return Row + ' ' + Column;
            }
        }

        private readonly object locker = new object();
        private readonly SortedList<MatrixEntry, int> matrix = new SortedList<MatrixEntry, int>();
        
        private string directory;
        private const string fileName = "CooccurenceMatrix.txt";

        private const int MAX_WORD_LENGTH = 3;
        private const int MAX_COOCCURRENCE_WORDS_COUNT = 100;

        public void Initialize(String directory)
        {
            lock (locker)
            {
                matrix.Clear();
                this.directory = directory;
                ReadMatrixFromFile();
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
                    for (int i = 1; i < parts.Count(); i ++)
                    {
                        var splits = parts[i].Split(':');
                        var column = splits[0];
                        var count = Int32.Parse(splits[1]);
                        matrix.Add(CreateEntry(row, column), count);
                    }                
                }
            }
        }

        private string GetMatrixFilePath()
        {
            return Path.Combine(directory, fileName);
        }

        private void WriteMatrixToFile()
        {
            var sb = new StringBuilder();
            var groups = matrix.GroupBy(p => p.Key.Row);
            foreach (var group in groups)
            {
                var row = group.First().Key.Row;
                var line = row + ' ' + group.Select(g => g.Key.Column + ":" + g.Value).
                    Aggregate((s1, s2) => s1 + ' ' +s2);
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
                int count;
                var have = matrix.TryGetValue(CreateEntry(word1, word2), out count);
                return have ? count : 0;
            }
        }

        public void HandleCoOcurrentWords(IEnumerable<String> words)
        {
            var list = LimitWordNumber(FilterOutBadWords(words).
                Distinct().ToList()).ToList();
            var allEntries = new List<MatrixEntry>();
            for (int i = 0; i < list.Count; i ++)
            {
                var word1 = list.ElementAt(i);
                for (int j = i; j < list.Count; j ++)
                {
                    var word2 = list.ElementAt(j);
                    allEntries.Add(CreateEntry(word1, word2));
                }
            }
            AddMatrixEntriesSync(allEntries);
        }


        private IEnumerable<String> LimitWordNumber(List<string> words)
        {
            return (words.Count > MAX_COOCCURRENCE_WORDS_COUNT)
                       ? words.GetRange(0, MAX_COOCCURRENCE_WORDS_COUNT)
                       : words;
        }


        private void AddMatrixEntriesSync(IEnumerable<MatrixEntry> allEntries)
        {
            lock (locker)
            {
                foreach (var target in allEntries)
                {
                    if (matrix.ContainsKey(target))
                    {
                        matrix[target]++;
                    }
                    else
                    {
                        matrix.Add(target, 1);
                    }
                }
            }
        }

        private IEnumerable<String> FilterOutBadWords(IEnumerable<String> words)
        {
            return words.Where(w => w.Length >= MAX_WORD_LENGTH 
                || w.Contains(' ') || w.Contains(':'));
        }


        private MatrixEntry CreateEntry(String word1, String word2)
        {
            word1 = Preprocess(word1);
            word2 = Preprocess(word2);
            if (word1.CompareTo(word2) > 0)
            {
                var temp = word2;
                word2 = word1;
                word1 = temp;
            }
            return new MatrixEntry(word1, word2);
        }

        public void Dispose()
        {
            lock (locker)
            {
                WriteMatrixToFile();
                matrix.Clear();
            }
        }
    }
}
