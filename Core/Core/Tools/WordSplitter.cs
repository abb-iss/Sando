using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
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

            MatchCollection matchCollection = Regex.Matches(searchTerms, quotesPattern);
            List<string> matches = new List<string>();
            foreach (Match match in matchCollection)
            {
                string currentMatch = match.Value;//.Trim('"', ' ');
                searchTerms = searchTerms.Replace(match.Value, "");
                if (!String.IsNullOrWhiteSpace(currentMatch))
                    matches.Add(currentMatch);
            }
            searchTerms = Regex.Replace(searchTerms, pattern, " ");
            searchTerms = Regex.Replace(searchTerms, @"(-{0,1})([A-Z][a-z]+)", " $1$2");
            searchTerms = Regex.Replace(searchTerms, @"(-{0,1})([A-Z]+|[0-9]+)", " $1$2");

            searchTerms = searchTerms.Replace("\"", " ");
            matches.AddRange(searchTerms.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            for(int i = 0; i < matches.Count; ++i)
            {
                string lower = matches[i].Trim().ToLower();
                matches[i] = Regex.Replace(lower, @"[ ]{2,}", " ");
            }
            return matches.Distinct().ToList();
        }

        public static bool InvalidCharactersFound(string searchTerms)
        {
            MatchCollection matchCollection = Regex.Matches(searchTerms, quotesPattern);
            foreach(Match match in matchCollection)
            {
                searchTerms = searchTerms.Replace(match.Value, "");
            }
            searchTerms = searchTerms.Replace("\"", " ");
            return Regex.IsMatch(searchTerms, pattern);
        }

        private static string pattern = "[^a-zA-Z0-9\\s\\*\\-]";
        private static string quotesPattern = "-{0,1}\"[^\"]+\"";
    }
}
