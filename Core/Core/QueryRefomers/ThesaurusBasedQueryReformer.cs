using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.Core.Tools;

namespace Sando.Core.QueryRefomers
{
    internal abstract class SynonymBasedWordReformer : AbstractWordReformer
    {
        protected SynonymBasedWordReformer(DictionaryBasedSplitter localDictionary) : 
            base(localDictionary)
        {
        }

        protected override IEnumerable<ReformedWord> GetReformedTargetInternal(String word)
        {
            var thesaurus = GetThesaurus();
            var list = thesaurus.GetSynonyms(word).OrderByDescending(s => s.SimilarityScore).
                Select(s => new SynonymReformedWord (GetTermChangeCategory(), word, 
                    s.Synonym, GetReformMessage(word, s.Synonym), 
                        s.SimilarityScore)).ToList();
            return list;
        }

        protected abstract string GetReformMessage(string originalWord, string newWord);
        protected abstract IThesaurus GetThesaurus();
        protected abstract TermChangeCategory GetTermChangeCategory();
    }


    internal class SeThesaurusWordReformer : SynonymBasedWordReformer
    {
        public SeThesaurusWordReformer(DictionaryBasedSplitter localDictionary) : 
            base(localDictionary)
        {
        }

        protected override string GetReformMessage(string originalWord, string newWord)
        {
            return "Find synonym of \"" + originalWord + "\" with \"" + newWord + "\"";
        }

        protected override IThesaurus GetThesaurus()
        {
            return SeSpecificThesaurus.GetInstance();
        }

        protected override TermChangeCategory GetTermChangeCategory()
        {
            return TermChangeCategory.SE_SYNONYM;
        }

        protected override int GetMaximumReformCount()
        {
            return QuerySuggestionConfigurations.SYNONYMS_MAX_COUNT;
        }
    }

    internal class GeneralThesaurusWordReformer : SynonymBasedWordReformer
    {
        public GeneralThesaurusWordReformer(DictionaryBasedSplitter localDictionary) 
            : base(localDictionary)
        {
        }

        protected override string GetReformMessage(string originalWord, string newWord)
        {
            return "Find synonym of \"" + originalWord + "\" with \"" + newWord + "\"";
        }

        protected override IThesaurus GetThesaurus()
        {
            return GeneralEnglishThesaurus.GetInstance();
        }

        protected override TermChangeCategory GetTermChangeCategory()
        {
            return TermChangeCategory.GENERAL_SYNONYM;
        }

        protected override int GetMaximumReformCount()
        {
            return QuerySuggestionConfigurations.SYNONYMS_MAX_COUNT;
        }
    }
}
