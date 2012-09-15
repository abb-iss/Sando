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
            word = Regex.Replace(word, @"([A-Z][a-z]+)", "_$1");
            word = Regex.Replace(word, @"([A-Z]+|[0-9]+)", "_$1");
            word = word.Replace(" _", "_");
            char[] delimiters = new char[] { '_', ':' };
            return word.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        }

        public static List<string> ExtractSearchTerms(string searchTerms)
        {
            Contract.Requires(searchTerms != null, "WordSplitter:ExtractSearchTerms - searchTerms cannot be null!");

            searchTerms = Regex.Replace(searchTerms, pattern, " ");

            MatchCollection matchCollection = Regex.Matches(searchTerms, "\"[^\"]+\"");
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
            matches.ForEach(m => m.Trim().ToLower());
            return matches;
        }

        public static bool InvalidCharactersFound(string searchTerms)
        {
            return Regex.IsMatch(searchTerms, pattern);
        }

        private static string pattern = "[^a-zA-Z0-9\\s\"\\*]";
    }
}
