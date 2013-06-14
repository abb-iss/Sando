using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using SF.Snowball.Ext;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Core.Tools
{
    public class DictionaryHelper
    {
        private static readonly Regex _quotesPattern = new Regex("-{0,1}\"[^\"]+\"", RegexOptions.Compiled);
        private static readonly Regex _patternChars = new Regex(@"([A-Z][a-z]+)", RegexOptions.Compiled);
        private static readonly Regex _patternCharsLowerCase = new Regex(@"([^a-zA-Z][a-z]+)", RegexOptions.Compiled);

        public static IEnumerable<String> ExtractElementWords(ProgramElement element)
        {
            if (element as ClassElement != null)
            {
                return ExtractClassWords(element as ClassElement);
            }
            if (element as CommentElement != null)
            {
                return ExtractCommentWords(element as CommentElement);
            }
            if (element as EnumElement != null)
            {
                return ExtractEnumWords(element as EnumElement);
            }
            if (element as FieldElement != null)
            {
                return ExtractFieldWords(element as FieldElement);
            }
            if (element as MethodElement != null)
            {
                return ExtractMethodWords(element as MethodElement);
            }
            if (element as MethodPrototypeElement != null)
            {
                return ExtractMethodPrototypeWords(element as MethodPrototypeElement);
            }
            if (element as PropertyElement != null)
            {
                return ExtractPropertyWords(element as PropertyElement);
            }
            if (element as StructElement != null)
            {
                return ExtractStructWords(element as StructElement);
            }
            if (element as TextLineElement != null)
            {
                return ExtractTextLineElement(element as TextLineElement);
            }
            if (element as XmlXElement != null)
            {
                return ExtractXmlWords(element as XmlXElement);
            }

            if (element.GetCustomProperties().Count > 0)
            {
                return ExtractUnknownElementWords(element);
            }

            //if this code is reached, contract will fail
            return null;
        }

        public static IEnumerable<String> GetQuotedStrings(String text)
        {
            return GetMatchedWords(_quotesPattern, text);
        }

        private static IEnumerable<String> ExtractMethodWords(MethodElement element)
        {
            return GetDefaultLetterWords(element.RawSource);
        }

        private static IEnumerable<String> ExtractXmlWords(XmlXElement element)
        {
            return GetDefaultLetterWords(element.Body);
        }

        private static IEnumerable<String> ExtractCommentWords(CommentElement element)
        {
            return GetDefaultLetterWords(element.Body);
        }

        private static IEnumerable<String> ExtractClassWords(ClassElement element)
        {
            return GetDefaultLetterWords(element.Name + " " + element.Namespace);
        }

        private static IEnumerable<String> ExtractEnumWords(EnumElement element)
        {
            return GetDefaultLetterWords(element.Body);
        }

        private static IEnumerable<string> ExtractFieldWords(FieldElement element)
        {
            return GetDefaultLetterWords(new [] {element.Name, element.FieldType});
        }

        private static IEnumerable<string> ExtractPropertyWords(PropertyElement element)
        {
            return GetDefaultLetterWords(element.Body);
        }

        private static IEnumerable<string> ExtractMethodPrototypeWords(MethodPrototypeElement
            element)
        {
            return GetDefaultLetterWords(new []{element.Arguments, element.Name});
        }

        private static IEnumerable<string> ExtractTextLineElement(TextLineElement element)
        {
            return GetDefaultLetterWords(element.Body);
        }

        private static IEnumerable<string> ExtractStructWords(StructElement element)
        {
            return GetDefaultLetterWords(element.Name + " " + element.Namespace);
        }

        private static IEnumerable<string> ExtractUnknownElementWords(ProgramElement element)
        {
            return GetDefaultLetterWords(element.RawSource);
        }


        private static IEnumerable<String> GetDefaultLetterWords(IEnumerable<string> codes)
        {
            return codes.SelectMany(GetDefaultLetterWords);
        }

        private static IEnumerable<String> GetDefaultLetterWords(String code)
        {
            var words = new List<String>();
            words.AddRange(GetMatchedWords(_patternChars, code));
            words.AddRange(GetMatchedWords(_patternCharsLowerCase, code).Select
                (TrimNonLetterPrefix));
            return words;
        }

        private static String TrimNonLetterPrefix(String word)
        {
            var firstLetter = word.First(Char.IsLetter);
            return word.Substring(word.IndexOf(firstLetter));
        }

        private static IEnumerable<string> GetMatchedWords(Regex pattern, String code)
        {
            var matches = pattern.Matches(code);
            return matches.Cast<Match>().Select(m => m.Groups[0].Value);
        }

        public static IEnumerable<int> GetQuoteStarts(string text)
        {
            var matches = RemoveChildMatches(_quotesPattern.Matches(text).Cast<Match>());
            return matches.Select(m => m.Groups[0].Index);
        }

        public static IEnumerable<int> GetQuoteEnds(string text)
        {
            var matches = RemoveChildMatches(_quotesPattern.Matches(text).Cast<Match>());
            return matches.Select(m => m.Groups[0].Index + m.Groups[0].Length - 1);
        }


        public static string GetStemmedQuery(String query)
        {
            var stemmer = new EnglishStemmer();
            stemmer.SetCurrent(query);
            stemmer.Stem();
            return stemmer.GetCurrent();
        }

        private static IEnumerable<Match> RemoveChildMatches(IEnumerable<Match> matches)
        {
            var simplifiedMatches = matches.ToList();
            foreach (var match in matches)
            {
                if(matches.Any(m => IsMatchIncluding(m, match)))
                {
                    simplifiedMatches.Remove(match);
                }
            }
            return simplifiedMatches;
        }

        private static Boolean IsMatchIncluding(Match m1, Match m2)
        {
            return !m1.Equals(m2) && m1.Groups[0].Index <= m2.Groups[0].Index &&
                   m1.Groups[0].Length + m1.Groups[0].Index >= m2.Groups[0].Length + m2.Groups[0].Index;
        }
    }
}
