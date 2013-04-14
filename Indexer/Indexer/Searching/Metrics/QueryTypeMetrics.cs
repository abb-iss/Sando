using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sando.Indexer.Searching.Metrics
{
    public class QueryTypeMetrics
    {
        public QueryTypeMetrics(string q)
        {
            query = q;
        }

        public int NumberOfTerms()
        {
            string [] terms = query.Split(' ');
            return terms.Count();
        }

        public int NumberOfCamelCaseTerms()
        {
            int numberOfCamelCaseTerms = 0;
            string[] terms = query.Split(' ');
            foreach (var term in terms)
            {
                if (_patternChars.IsMatch(term))
                    numberOfCamelCaseTerms++;
            }

            return numberOfCamelCaseTerms;
        }

        public bool IsQuoted()
        {
            return _patternQuotes.IsMatch(query);
        }

        private string query;
        private Regex _patternQuotes = new Regex("-{0,1}\"[^\"]+\"", RegexOptions.Compiled);
        private Regex _patternChars = new Regex(@"([A-Z][a-z]+)", RegexOptions.Compiled);
    }
}
