using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Core.Tools
{
    internal class DictionaryBuilder
    {

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
    }
}
