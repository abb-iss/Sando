using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.Core.Tools;

namespace Sando.Core.QueryRefomers
{
    internal class AcronymExpander
    {
        private readonly DictionaryBasedSplitter localDictionary;

        public AcronymExpander(DictionaryBasedSplitter localDictionary)
        {
            this.localDictionary = localDictionary;
        }

        private class ExtendedAcronym
        {
            private List<String> Words;

            public ExtendedAcronym(IEnumerable<string> Words)
            {
                this.Words = Words.ToList();
            }

            public IEnumerable<ExtendedAcronym> GetCommonCoOccurWords(IWordCoOccurrenceMatrix 
                matrix, char letter) 
            {
                return Words.Select(matrix.GetCoOccurredWordsAndCount).Aggregate
                    ((d1, d2) => d1.Keys.Intersect(d2.Keys).ToDictionary(k => k, v => 0)).
                        Keys.Where(k => k.StartsWith(letter.ToString())).Select(k => 
                            new ExtendedAcronym(Words.AddImmutably(k)));
            }

            public int ComputeCoOccurrenceCount(IWordCoOccurrenceMatrix matrix)
            {
                int sum = 0;
                for (int i = Words.Count - 1; i > 0; i--)
                {
                    var current = Words.ElementAt(i);
                    var restList = Words.GetRange(0, i);
                    sum += restList.Sum(rest => matrix.GetCoOccurrenceCount(rest, current));
                }
                return sum;
            }

            public IReformedQuery ToReformedQuery(IWordCoOccurrenceMatrix matrix)
            {
                return new ExpandedQuery(this.Words, ComputeCoOccurrenceCount(matrix));   
            }


            private class ExpandedQuery : IReformedQuery
            {
                internal ExpandedQuery(IEnumerable<String> expandedWords, int CoOccurrenceCount)
                {
                    this.CoOccurrenceCount = CoOccurrenceCount;
                    this.ReformedWords = expandedWords.Select(ew => new ReformedWord(TermChangeCategory.
                        ACRONYM_EXPAND, ew.First().ToString(), ew, "")).ToArray();
                    this.WordsAfterReform = this.ReformedWords.Select(rw => rw.NewTerm).ToArray();
                    this.QueryString = this.WordsAfterReform.Aggregate((w1, w2) => w1 + " " + w2);
                    this.EditDistance = this.WordsAfterReform.Sum(s => s.Count() - 1);
                    this.ReformExplanation = "Expanding an acronym.";

                }

                public bool Equals(IReformedQuery other)
                {
                    if (other as ExpandedQuery != null)
                    {
                        var wordsList1 = this.WordsAfterReform.ToArray();
                        var wordsList2 = other.WordsAfterReform.ToArray();
                        return wordsList1.Aggregate((s1, s2) => s1 + " " + s2).Equals
                            (wordsList2.Aggregate((s1, s2) => s1 + " " + s2));
                    }
                    return false;
                }

                public IEnumerable<ReformedWord> ReformedWords { get; private set; }
                public IEnumerable<string> WordsAfterReform { get; private set; }
                public string ReformExplanation { get; private set; }
                public string QueryString { get; private set; }
                public int CoOccurrenceCount { get; private set; }
                public int EditDistance { get; private set; }
            }
        }

        public IEnumerable<IReformedQuery> GetExpandedQueries(string target)
        {
            if (!IsPreconditionMet(target)) return Enumerable.Empty<IReformedQuery>();
            var entries = localDictionary.GetEntries(en => IsEntryStartWith(en, target[0], target[1])
                && !en.Column.Equals(en.Row)).ToList();
            var acronyms = CreateInitialAcronym(entries, target[0], target[1]);

            for (int i = 2; i < target.Count(); i++)
            {
                acronyms = acronyms.SelectMany(a => a.GetCommonCoOccurWords(localDictionary, 
                    target.ElementAt(i)));
            }
            return acronyms.Select(a => a.ToReformedQuery(localDictionary)).
                OrderByDescending(query => query.CoOccurrenceCount).
                    TrimIfOverlyLong(GetMaximumCount());
        }


        private IEnumerable<ExtendedAcronym> CreateInitialAcronym(IEnumerable<IMatrixEntry> entries, 
            char c1, char c2)
        {
            var list = new List<ExtendedAcronym>();
            foreach (var entry in entries)
            {
                var firstWord = entry.Row.StartsWith(c1.ToString()) ? entry.Row : entry.Column;
                var secondWord = firstWord.Equals(entry.Row) ? entry.Column : entry.Row;
                list.Add(new ExtendedAcronym(new string[]{firstWord, secondWord}));
            }
            return list;
        }

        private bool IsPreconditionMet(string word)
        {
            return word.Count() >= 2;
        }

        private bool IsEntryStartWith(IMatrixEntry entry, char c1, char c2)
        {
            return entry.Column.StartsWith(c1.ToString()) && entry.Row.StartsWith(c2.ToString()) ?
                true : entry.Column.StartsWith(c2.ToString()) && entry.Row.StartsWith(c1.ToString());
        }

        private int GetMaximumCount()
        {
            return 3;
        }
    }
}
