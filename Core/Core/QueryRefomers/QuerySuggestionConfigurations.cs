using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.QueryRefomers
{
    public class QuerySuggestionConfigurations
    {
        // Only when the word is longer than this value, our reformers try to find
        // a recommendation to it.
        public const int MINIMUM_WORD_LENGTH_TO_REFORM = 3;

        // The maximum of corrections retrieved for a misspelled word.
        public const int SIMILAR_WORDS_MAX_COUNT = 5;

        // The maximum of synonyms retrieved from a thesaurus.
        public const int SYNONYMS_MAX_COUNT = 3;

        // The maxium number of words that are suggested by the cooccurrence reformer.
        public const int COOCCURRENCE_WORDS_MAX_COUNT = 5;

        // The maximum of queries collected.
        public const int MAXIMUM_RECOMMENDATIONS_COUNT = 30;

        // The maximum of queries shown at the user interface.
        public const int MAXIMUM_RECOMMENDED_QUERIES_IN_USER_INTERFACE = 6;
    }
}
