using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using Sando.ExtensionContracts.SplitterContracts;

namespace Sando.Core.Tools
{
    public class WordSplitter : IWordSplitter
    {
        public string[] ExtractWords(string word)
        {
            word = Regex.Replace(word, @"([A-Z][a-z]+|[A-Z]+|[0-9]+)", "_$1").Replace(" _", "_");
            char[] delimiters = new char[] { '_', ':' };
            return word.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        }

        public static List<string> ExtractSearchTerms(string searchTerms)
        {
            Contract.Requires(searchTerms != null, "WordSplitter:ExtractSearchTerms - searchTerms cannot be null!");

            searchTerms = Regex.Replace(searchTerms, @"\\|/|:|~|?", "");

            MatchCollection matchCollection = Regex.Matches(searchTerms, "\"[^\"]*\"");
            List<string> matches = new List<string>();
            foreach (Match match in matchCollection)
            {
                string currentMatch = match.Value.Trim('"', ' ');
                searchTerms = searchTerms.Replace(match.Value, "");
                if (!String.IsNullOrWhiteSpace(currentMatch))
                    matches.Add(currentMatch);
            }
            searchTerms = searchTerms.Replace("\"", " ");
            matches.AddRange(searchTerms.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            return matches;
        }
    }
}
