using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sando.Indexer.Searching.Metrics
{
    public static class QueryMetrics
    {
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
                    continue;
                }
                else if (terms[i] == "\"")
                {
                    quotesOn = !quotesOn;
                    continue;
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
                        terms[i] = terms[i].TrimEnd('"');
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

        private static Regex _patternCamel = new Regex("([A-Z][a-z]+)", RegexOptions.Compiled);
        private static Regex _patternAcronym = new Regex("[A-Z]{3}", RegexOptions.Compiled);
    }
}
