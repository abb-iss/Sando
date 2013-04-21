using Sando.ExtensionContracts.ProgramElementContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sando.Indexer.Searching.Metrics
{
    public static class QueryMetrics
    {
        static QueryMetrics()
        {
            SavedQuery = String.Empty;
        }

        public static QueryTermTypeList ExamineQuery(string query)
        {
            string[] terms = query.Split(' ');
            QueryTermTypeList queryTypes = new QueryTermTypeList();
            bool quotesOn = false;
            for (int i = 0, j = 0; i < terms.Length; i++)
            {
                if (quotesOn)
                {
                    queryTypes[j] |= QueryTermType.Quoted;
                }
                else
                {
                    queryTypes[j] = QueryTermType.None;
                }

                if (terms[i] == "-")
                {
                    queryTypes[j+1] |= QueryTermType.Minus;
                }
                else if (terms[i] == "\"")
                {
                    quotesOn = !quotesOn;
                }
                else
                {
                    if (terms[i].StartsWith("-"))
                    {
                        queryTypes[j] |= QueryTermType.Minus;
                    }
                    if (terms[i].StartsWith("\""))
                    {
                        quotesOn = !quotesOn;
                        if (quotesOn)
                        {
                            queryTypes[j] |= QueryTermType.Quoted;
                        }
                        else
                        {
                            queryTypes[j] ^= QueryTermType.Quoted;
                        }
                    }
                    if (terms[i].StartsWith("filetype\\:"))
                    {
                        queryTypes[j] |= QueryTermType.Filetype;
                    }

                    if (terms[i].EndsWith("\""))
                    {
                        quotesOn = !quotesOn;
                    }
                    if (_patternCamel.IsMatch(terms[i]))
                    {
                        queryTypes[j] |= QueryTermType.Camelcase;
                    }
                    if (_patternAcronym.IsMatch(terms[i]))
                    {
                        queryTypes[j] |= QueryTermType.Acronym;
                    }
                    if (terms[i].Contains('_'))
                    {
                        queryTypes[j] |= QueryTermType.Underscore;
                    }
                    j++;
                }
            }
            return queryTypes;
        }

        public static string DescribeQueryProgramElementMatch(ProgramElement progElem, string query)
        {
            if (progElem.Name.Contains(query))
            {
                return "Name_ContainsQuery";
            }
            else if (progElem.RawSource.Contains(query))
            {
                return "RawSource_ContainsQuery";
            }
            else
            {
                return "NoMatchWithQuery";
            }
        }

        public static double DiceCoefficient(string query1, string query2)
        {
            string[] terms1 = query1.Split(' ');
            string[] terms2 = query2.Split(' ');
            terms1 = terms1.Where(t => _patternChars.IsMatch(t)).ToArray();
            terms2 = terms2.Where(t => _patternChars.IsMatch(t)).ToArray();
            if (terms1.Count() == 0 || terms2.Count() == 0) return 0.0;
            return (2.0 * terms1.Intersect(terms2).Count()) / (terms1.Count() + terms2.Count());
        }

        public static string SavedQuery { get; set; }

        private static Regex _patternChars = new Regex("([a-zA-Z])", RegexOptions.Compiled);
        private static Regex _patternCamel = new Regex("([A-Z][a-z]+)", RegexOptions.Compiled);
        private static Regex _patternAcronym = new Regex("([A-Z]{3})", RegexOptions.Compiled);
    }
}
