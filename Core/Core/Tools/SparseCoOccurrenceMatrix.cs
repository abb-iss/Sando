using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Sando.Core.Tools
{
    public class SparseCoOccurrenceMatrix : IWordCoOccurrenceMatrix
    {
        private readonly List<String> allWords = new List<string>();
        private readonly List<BoxedInt> A = new List<BoxedInt>();
        private readonly List<BoxedInt> IA = new List<BoxedInt>();
        private readonly List<BoxedInt> JA = new List<BoxedInt>();
        private readonly object locker = new object();

        public SparseCoOccurrenceMatrix()
        {
            TimedProcessor.GetInstance().AddTimedTask(SaveToFile, 13 * 60 * 1000);
        }

        private class BoxedInt : IComparable<BoxedInt>
        {
            public int Value { get; set; }

            public BoxedInt(int Value)
            {
                this.Value = Value;
            }

            public int CompareTo(BoxedInt other)
            {
                return this.Value.CompareTo(other.Value);
            }
        }

        private int GetValue(int row, int column)
        {
            var columnNumbers = GetNonZeroColumnNumbers(row).ToList();
            var allRow = A.GetRange(IA.ElementAt(row).Value, columnNumbers.Count);
            var index = columnNumbers.BinarySearch(new BoxedInt(column));
            return index >= 0 ? allRow.ElementAt(index).Value : 0;
        }

        private IEnumerable<BoxedInt> GetNonZeroColumnNumbers(int row)
        {
            var start = IA.ElementAt(row).Value;
            var length = row + 1 < IA.Count ? IA.ElementAt(row + 1).Value - start 
                : JA.Count - start;
            return JA.GetRange(start, length);
        }

        private void InsertRowAndColumnAt(int index, int value)
        {
            // Update A.
            var AIndex = IA.Count > index && index >= 0 ? IA.ElementAt(index).Value :
                index - 1 >= 0 && index - 1 < IA.Count ? IA.ElementAt(index - 1).Value 
                    + GetNonZeroColumnNumbers(index - 1).Count() : 0;
            A.Insert(AIndex, new BoxedInt(value));

            // Update IA.
            IA.Insert(index, new BoxedInt(AIndex));
            for (int i = index + 1; i < IA.Count; i++)
            {
                IA.ElementAt(i).Value ++;
            }

            // Update JA.
            for (int i = 0; i < JA.Count; i++)
            {
                JA.ElementAt(i).Value = JA.ElementAt(i).Value >= index
                    ? JA.ElementAt(i).Value + 1
                        : JA.ElementAt(i).Value;
            }
            JA.Insert(AIndex, new BoxedInt(index));
        }

        private void IncrementCell(int row, int column)
        {
            var columns = GetNonZeroColumnNumbers(row).ToList();
            var index = columns.BinarySearch(new BoxedInt(column));
            if (index >= 0)
            {
                A.ElementAt(IA.ElementAt(row).Value + index).Value++;
                return;
            }
            
            // Update A
            var position = ~index;
            var AIndex = position + IA.ElementAt(row).Value;
            A.Insert(AIndex, new BoxedInt(1));

            // Update IA
            for (int i = row + 1; i < IA.Count; i ++)
            {
                IA.ElementAt(i).Value++;
            }

            // Udpate JA
            JA.Insert(AIndex, new BoxedInt(column));
        }


        private const int GRAM_NUMBER = 3;
        private const int MAX_WORD_LENGTH = 3;
        private const int MAX_COOCCURRENCE_WORDS_COUNT = 100;

        private readonly WorkQueueBasedProcess queue = new WorkQueueBasedProcess();
        private const string fileName = "SparseMatrix.txt";
        private string directory;

       
        public int GetCoOccurrenceCount(string word1, string word2)
        {
            lock (locker)
            {
                var row = allWords.BinarySearch(word1);
                var column = allWords.BinarySearch(word2);
                return row >= 0 && column >= 0 ? GetValue(row, column) : 0;
            }
        }

        public void Initialize(string directory)
        {
            lock (locker)
            {
                this.directory = directory;
                ClearMemory();
                ReadFromFile();
            }            
        }

        private void ReadFromFile()
        {
            if (File.Exists(GetMatrixFilePath()))
            {
                var lines = File.ReadAllLines(GetMatrixFilePath());
                allWords.AddRange(lines[0].Split());
                A.AddRange(lines[1].Split().Select(i => new BoxedInt(int.Parse(i))));
                IA.AddRange(lines[2].Split().Select(i => new BoxedInt(int.Parse(i))));
                JA.AddRange(lines[3].Split().Select(i => new BoxedInt(int.Parse(i))));
            }
        }

        private void ClearMemory()
        {
            this.allWords.Clear();
            this.A.Clear();
            this.IA.Clear();
            this.JA.Clear();
        }

        public void Dispose()
        {
            lock (locker)
            {
                TimedProcessor.GetInstance().RemoveTimedTask(SaveToFile);
                SaveToFile();
                ClearMemory();
            }
        }

        private void SaveToFile()
        {
            lock (locker)
            {
                if (Directory.Exists(directory) && allWords.Any())
                {
                    var lineOne = allWords.Aggregate(new StringBuilder(), (builder, word) => builder.Append(word).Append(" "));                    
                    var lineTwo = A.Select(i => i.Value.ToString()).Aggregate(new StringBuilder(), (builder, number) => builder.Append(number).Append( " "));
                    var lineThree = IA.Select(i => i.Value.ToString()).Aggregate(new StringBuilder(), (builder, number) => builder.Append(number).Append( " " ));
                    var lineFour = JA.Select(i => i.Value.ToString()).Aggregate(new StringBuilder(), (builder, number) => builder.Append(number).Append(" "));
                    var lines = new string[] {lineOne.ToString().TrimEnd(), lineTwo.ToString().TrimEnd(), lineThree.ToString().TrimEnd(), lineFour.ToString().TrimEnd()};
                    File.WriteAllLines(GetMatrixFilePath(), lines);
                }
            }
        }

        public void HandleCoOcurrentWordsAsync(IEnumerable<String> words)
        {
            queue.Enqueue(n => { 
                HandleCoOcurrentWordsSync(n);
                return 0;
            }, words);
        }

        public Dictionary<string, int> GetCoOccurredWordsAndCount(string word)
        {
            lock (locker)
            {
                var row = allWords.BinarySearch(word);
                if (row < 0) return new Dictionary<string, int>();
                var columns = GetNonZeroColumnNumbers(row);
                return columns.Select(c => allWords.ElementAt(c.Value)).ToDictionary(k => k,
                    k => GetCoOccurrenceCount(k, word));
            }
        }

        public Dictionary<string, int> GetAllWordsAndCount()
        {
            lock (locker)
            {
                return allWords.ToDictionary(w => w, w => GetCoOccurrenceCount(w, w));
            }
        }

        public IEnumerable<IMatrixEntry> GetEntries(Predicate<IMatrixEntry> predicate)
        {
            lock (locker)
            {
                var results = new List<IMatrixEntry>();
                for (int i = 0; i < JA.Count; i++)
                {
                    var row = GetRowByAIndex(i);
                    var column = JA.ElementAt(i).Value;
                    var count = A.ElementAt(i).Value;

                    var rowWord = allWords.ElementAt(row);
                    var columnWord = allWords.ElementAt(column);
                    var entry = new MatrixEntry(rowWord, columnWord, count);
                    if (predicate.Invoke(entry)) results.Add(entry);
                }
                return results;
            }
        }

        private int GetRowByAIndex(int aIndex)
        {
            var IAIndex = IA.BinarySearch(new BoxedInt(aIndex));
            return IAIndex >= 0 ? IAIndex : ~IAIndex - 1;
        }

        public void HandleCoOcurrentWordsSync(IEnumerable<string> words)
        {
            lock (locker)
            {
                words = SelectWords(words.ToList()).ToList();

                // Add new words.
                var newWords = words.Except(allWords);
                foreach (var nw in newWords)
                {
                    var index = allWords.BinarySearch(nw);
                    InsertRowAndColumnAt(~index, 1);
                    allWords.Insert(~index, nw);
                }

                // Increment occurrence.
                var entries = GetBigramEntries(words);
                foreach (var entry in entries)
                {
                    int rowNumber, columnNumber;
                    ComputeWordPosition(entry.Row, entry.Column, out rowNumber, 
                        out columnNumber);
                    IncrementCell(rowNumber, columnNumber);
                }
            }
        }

        private void ComputeWordPosition(String rowWord, string columnWord, 
            out int row, out int column)
        {
            row = allWords.BinarySearch(rowWord);
            column = allWords.BinarySearch(columnWord);
        }

        private IEnumerable<MatrixEntry> GetBigramEntries(IEnumerable<string> words)
        {
            var list = words.ToList();
            var allEntries = new List<MatrixEntry>();
            int i;
            for (i = 0; i + GRAM_NUMBER - 1 < list.Count; i++)
            {
                allEntries.AddRange(InternalGetEntries(list.GetRange(i, GRAM_NUMBER)));
            }

            // Check if having leftovers.
            if (i + GRAM_NUMBER - 1 != list.Count - 1 && list.Any())
            {
                allEntries.AddRange(InternalGetEntries(list.GetRange(i, list.Count - i)));
            }

            return allEntries;
        }

        private IEnumerable<MatrixEntry> InternalGetEntries(IEnumerable<string> words)
        {
            var list = words.ToList();
            var allEntries = new List<MatrixEntry>();
            for (int i = 0; i < list.Count; i++)
            {
                var word1 = list.ElementAt(i);
                for (int j = i; j < list.Count; j++)
                {
                    var word2 = list.ElementAt(j);
                    allEntries.Add(new MatrixEntry(word1, word2));
                    allEntries.Add(new MatrixEntry(word2, word1));
                }
            }
            return allEntries;
        }


        private IEnumerable<String> SelectWords(List<string> words)
        {
            words = FilterOutBadWords(words).ToList();
            words = (words.Count > MAX_COOCCURRENCE_WORDS_COUNT)
                ? words.GetRange(0, MAX_COOCCURRENCE_WORDS_COUNT)
                   : words;
            return words.Distinct();
        }

        private IEnumerable<String> FilterOutBadWords(IEnumerable<String> words)
        {
            return words.Where(w => w.Length >= MAX_WORD_LENGTH
                || w.Contains(' ') || w.Contains(':'));
        }

        private class MatrixEntry : IMatrixEntry
        {
            public string Row { get; private set; }
            public string Column { get; private set; }
            public int Count { get; private set; }

            public MatrixEntry(String Row, String Column, int Count = 0)
            {
                this.Row = Row;
                this.Column = Column;
                this.Count = Count;
            }
        }

        private string GetMatrixFilePath()
        {
            return Path.Combine(directory, fileName);
        }
    }
}
