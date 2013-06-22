using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.Core.Tools;

namespace Sando.Core.QueryRefomers
{
    internal class CoOccurrenceBasedReformer : AbstractContextSensitiveWordReformer
    {
        private List<string> otherWords;

        public CoOccurrenceBasedReformer(DictionaryBasedSplitter localDictionary) : base(localDictionary)
        {
        }

        protected override IEnumerable<ReformedWord> GetReformedTargetInternal(string target)
        {
            if (localDictionary.DoesWordExist(target, DictionaryOption.IncludingStemming))
            {
                var commonWords = otherWords.Select(w => localDictionary.GetCoOccurredWordsAndCount(w).
                    Select(p => p.Key)).Aggregate((l1, l2) => l1.Intersect(l2));
                return commonWords.Select(w => new ReformedWord(TermChangeCategory.COOCCUR, target, 
                    w, GetMessage(target, w)));
            }
            return Enumerable.Empty<ReformedWord>();
        }

        private string GetMessage(string original, string reformed)
        {
            return "\"" + reformed + "\" is an often neighbor.";
        }

        public override void SetContextWords(IEnumerable<string> words)
        {
            this.otherWords = words.ToList();
        }
    }
}
