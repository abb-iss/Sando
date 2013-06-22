using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.QueryRefomers
{
    public class QuerySuggestionConfigurations
    {
        // The maximum of corrections retrieved for a misspelled word.
        public const int SIMILAR_WORDS_MAX_COUNT = 5;

        // The maximum of queries collected.
        public const int MAXIMUM_RECOMMENDATIONS_COUNT = 30;

        // The maximum of queries shown at the user interface.
        public const int MAXIMUM_RECOMMENDED_QUERIES_IN_USER_INTERFACE = 3;
    }
}
