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
            var delimiters = new[] { '_', ':' };
            return word.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        }


        public static List<string> ExtractSearchTerms(string searchTerms)
        {
            Contract.Requires(searchTerms != null, "WordSplitter:ExtractSearchTerms - searchTerms cannot be null!");

			//1.handle quotes
            var matchCollection = Regex.Matches(searchTerms, QuotesPattern);
            var matches = new List<string>();
            foreach (Match match in matchCollection)
            {
                string currentMatch = match.Value;//.Trim('"', ' ');
                searchTerms = searchTerms.Replace(match.Value, "");
                if (!String.IsNullOrWhiteSpace(currentMatch))
                    matches.Add(currentMatch);
            }

			//2.add unsplit terms
			var splitTerms = searchTerms.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			foreach(string term in splitTerms)
			{
				if(term.All(c => Char.IsUpper(c) || Char.IsLower(c)) || term.All(c => Char.IsLetter(c) || Char.IsNumber(c)))
				{
					matches.Add(term);
				}
			}

			//3.do rest...
            searchTerms = Regex.Replace(searchTerms, Pattern, " ");
            searchTerms = Regex.Replace(searchTerms, @"(-{0,1})([A-Z][a-z]+)", " $1$2");
            searchTerms = Regex.Replace(searchTerms, @"(-{0,1})([A-Z]+|[0-9]+)", " $1$2");

            searchTerms = searchTerms.Replace("\"", " ");
            matches.AddRange(searchTerms.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            for(int i = 0; i < matches.Count; ++i)
            {
                string lower = matches[i].Trim().ToLower();
                matches[i] = Regex.Replace(lower, @"[ ]{2,}", " ");
            }
            return matches.Distinct().ToList();
        }

        public static SortedSet<string> GetFileExtensions(string searchTerms)
        {
            //2a.add filetype extensions
            var matchCollection = Regex.Matches(searchTerms, FileExtensionPattern);
            var matches = new SortedSet<string>();
            foreach (Match match in matchCollection)
            {
                string currentMatch = match.Value;//.Trim('"', ' ');
                searchTerms = searchTerms.Replace(match.Value, "");
                if (!String.IsNullOrWhiteSpace(currentMatch))
                {
                    matches.Add(currentMatch.Substring(currentMatch.IndexOf(':')+1));                    
                }
            }
            return matches;
        }

        public static bool InvalidCharactersFound(string searchTerms)
        {
            MatchCollection matchCollection = Regex.Matches(searchTerms, QuotesPattern);
            foreach(Match match in matchCollection)
            {
                searchTerms = searchTerms.Replace(match.Value, "");
            }
            searchTerms = searchTerms.Replace("\"", " ");
            return Regex.IsMatch(searchTerms, Pattern);
        }

        private const string Pattern = "[^a-zA-Z0-9\\s\\*\\-]";
        private const string QuotesPattern = "-{0,1}\"[^\"]+\"";
        private const string FileExtensionPattern = "filetype\\:([a-zA-Z]\\w+)";
    }
}
