using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sando.Core.Tools
{
    public class SandoQueryParser
    {
        private const string QuoteMarker = "✉∞dq";
        private const string QuoteNormalizationRegex = @"\s*(""|\\"")\s*";
        private const string UnpairedQuoteRegex = @"(?<!✉∞dq)""(?!✉∞dq)";
        private const string LiteralSearchRegex = @"(?<literal>\-?""(?<literalcontent>[^""\\]*(?:\\.[^""\\]*)*)"")";
        private const string LocationWithLiteralsFilterRegex = @"(?<filter>(?<!\w)-?location:(?<location>(""✉∞dq[\w\:\.\\\(\)\[\]\{\}\* ]+✉∞dq"")))";
        private const string LocationWithoutLiteralsFilterRegex = @"(?<filter>(?<!\w)-?location:(?<location>[\w\:\.\\\(\)\[\]\{\}\*]+))";
        private const string FileExtensionFilterRegex = @"(?<filter>(?<!\w)-?file:\.?(?<fileext>\w+))";
        private const string ProgramElementTypeFilterRegex = @"(?<filter>(?<!\w)-?type:(?<type>field|method|property|enum|struct|class))";
        private const string AccessLevelFilterRegex = @"(?<filter>(?<!\w)-?access:(?<access>public|private|protected|internal))";
        private const string InvalidCharactersRegex = "[^a-zA-Z0-9_\\s\\*\\-]";
        private const string NormalSearchTermsRegex = @"(?<searchterm>\-?\w+[\*?\w]*)";
        private const string MultipleWildcardRegex = @"\*+";

        public SandoQueryDescription Parse(string query)
        {
            var sandoQueryDescription = new SandoQueryDescription {OriginalQuery = query};
            var analyzedQuery = query;
            if (String.IsNullOrWhiteSpace(analyzedQuery))
                return sandoQueryDescription;
            analyzedQuery = Regex.Replace(analyzedQuery, QuoteNormalizationRegex, "$1");
            analyzedQuery = MarkQuotes(analyzedQuery);
            var parseFunctions = GetQueryParseFunctions();
            foreach (var function in parseFunctions)
            {
                analyzedQuery = function(analyzedQuery, sandoQueryDescription);
                if (String.IsNullOrWhiteSpace(analyzedQuery))
                    return sandoQueryDescription;
            }
            return sandoQueryDescription;
        }

        private static string MarkQuotes(string query)
        {
            var matches = Regex.Matches(query, LiteralSearchRegex);
            foreach (Match match in matches)
            {
                var matchedLiteralContent = match.Groups["literalcontent"].Value;
                if (!String.IsNullOrWhiteSpace(matchedLiteralContent))
                {
                    var markedQuotedMatch = QuoteMarker + matchedLiteralContent + QuoteMarker;
                    query = query.Replace(matchedLiteralContent, markedQuotedMatch);
                }
            }
            query = Regex.Replace(query, UnpairedQuoteRegex, " ");
            return query;
        }

        private static IEnumerable<Func<string, SandoQueryDescription, string>> GetQueryParseFunctions()
        {
            return new List<Func<string, SandoQueryDescription, string>>
                {
                    ParseLocationWithLiteralsFilters,
                    ParseLiteralSearchTerms,
                    ParseLocationWithoutLiteralsFilters,
                    ParseFileExtensionFilters,
                    ParseProgramElementTypeFilters,
                    ParseAccessLevelFilters,
                    ParseNormalSearchTerms
                };
        }

        private static string ParseLocationWithLiteralsFilters(string query, SandoQueryDescription sandoQueryDescription)
        {
            var matches = Regex.Matches(query, LocationWithLiteralsFilterRegex, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var matchedFilter = match.Groups["filter"].Value;
                var matchedLocation = match.Groups["location"].Value.Replace(QuoteMarker, String.Empty);
                if (!String.IsNullOrWhiteSpace(matchedLocation))
                {
                    if (matchedFilter.StartsWith("-"))
                        sandoQueryDescription.Locations.Add("-" + matchedLocation);
                    else
                        sandoQueryDescription.Locations.Add(matchedLocation);
                }
                query = query.Replace(matchedFilter, " ");
            }
            query = Regex.Replace(query, QuoteMarker, String.Empty);
            return query;
        }

        private static string ParseLiteralSearchTerms(string query, SandoQueryDescription sandoQueryDescription)
        {
            var matches = Regex.Matches(query, LiteralSearchRegex, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var matchedLiteral = match.Groups["literal"].Value;
                var matchedLiteralContent = match.Groups["literalcontent"].Value;
                if (!String.IsNullOrWhiteSpace(matchedLiteralContent))
                {
                    sandoQueryDescription.LiteralSearchTerms.Add(matchedLiteral);
                }
                query = query.Replace(matchedLiteral, " ");
            }
            return query;
        }

        private static string ParseLocationWithoutLiteralsFilters(string query, SandoQueryDescription sandoQueryDescription)
        {
            var matches = Regex.Matches(query, LocationWithoutLiteralsFilterRegex, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var matchedFilter = match.Groups["filter"].Value;
                var matchedLocation = match.Groups["location"].Value.Replace(QuoteMarker, String.Empty);
                if (!String.IsNullOrWhiteSpace(matchedLocation))
                {
                    if (matchedFilter.StartsWith("-"))
                        sandoQueryDescription.Locations.Add("-" + matchedLocation);
                    else
                        sandoQueryDescription.Locations.Add(matchedLocation);
                }
                query = query.Replace(matchedFilter, " ");
            }
            query = Regex.Replace(query, QuoteMarker, String.Empty);
            return query;
        }

        private static string ParseFileExtensionFilters(string query, SandoQueryDescription sandoQueryDescription)
        {
            var matches = Regex.Matches(query, FileExtensionFilterRegex, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var matchedFilter = match.Groups["filter"].Value;
                var matchedFileExtension = match.Groups["fileext"].Value.ToLower();
                if (matchedFilter.StartsWith("-"))
                    sandoQueryDescription.FileExtensions.Add("-" + matchedFileExtension);
                else
                    sandoQueryDescription.FileExtensions.Add(matchedFileExtension);
                query = query.Replace(matchedFilter, " ");
            }
            return query;
        }

        private static string ParseProgramElementTypeFilters(string query, SandoQueryDescription sandoQueryDescription)
        {
            var matches = Regex.Matches(query, ProgramElementTypeFilterRegex, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var matchedFilter = match.Groups["filter"].Value;
                var matchedProgramElementType = match.Groups["type"].Value.ToLower();
                if (matchedFilter.StartsWith("-"))
                    sandoQueryDescription.ProgramElementTypes.Add("-" + matchedProgramElementType);
                else
                    sandoQueryDescription.ProgramElementTypes.Add(matchedProgramElementType);
                query = query.Replace(matchedFilter, " ");
            }
            return query;
        }

        private static string ParseAccessLevelFilters(string query, SandoQueryDescription sandoQueryDescription)
        {
            var matches = Regex.Matches(query, AccessLevelFilterRegex, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var matchedFilter = match.Groups["filter"].Value;
                var matchedAccessLevel = match.Groups["access"].Value.ToLower();
                if (matchedFilter.StartsWith("-"))
                    sandoQueryDescription.AccessLevels.Add("-" + matchedAccessLevel);
                else
                    sandoQueryDescription.AccessLevels.Add(matchedAccessLevel);
                query = query.Replace(matchedFilter, " ");
            }
            return query;
        }

        private static string ParseNormalSearchTerms(string query, SandoQueryDescription sandoQueryDescription)
        {
            query = Regex.Replace(query, InvalidCharactersRegex, " ");
            query = Regex.Replace(query, MultipleWildcardRegex, "*");
            var matches = Regex.Matches(query, NormalSearchTermsRegex);
            foreach (Match match in matches)
            {
                var matchedTerm = match.Groups["searchterm"].Value;
                sandoQueryDescription.SearchTerms.Add(matchedTerm);
                query = query.Replace(matchedTerm, " ");
            }
            return query;
        }
    }

    public class SandoQueryDescription
    {
        public SandoQueryDescription()
        {
            SearchTerms = new SortedSet<string>();
            LiteralSearchTerms = new SortedSet<string>();
            FileExtensions = new SortedSet<string>();
            ProgramElementTypes = new SortedSet<string>();
            Locations = new SortedSet<string>();
            AccessLevels = new SortedSet<string>();
        }

        public string OriginalQuery { get; set; }
        public SortedSet<string> SearchTerms { get; set; }
        public SortedSet<string> LiteralSearchTerms { get; set; }
        public SortedSet<string> FileExtensions { get; set; }
        public SortedSet<string> ProgramElementTypes { get; set; }
        public SortedSet<string> Locations { get; set; }
        public SortedSet<string> AccessLevels { get; set; }

        public bool IsValid
        {
            get
            {
                return SearchTerms.Any() ||
                       LiteralSearchTerms.Any() ||
                       FileExtensions.Any() ||
                       ProgramElementTypes.Any() ||
                       Locations.Any() ||
                       AccessLevels.Any();
            }
        }

        public override string ToString()
        {
            return String.Join(", ", GetDescriptionForCollections().Where(d => d != null));
        }

        private IEnumerable<string> GetDescriptionForCollections()
        {
            return new List<string>
                {
                    GetDescriptionForCollection("Locations", Locations),
                    GetDescriptionForCollection("Literal search terms", LiteralSearchTerms),
                    GetDescriptionForCollection("File extensions", FileExtensions),
                    GetDescriptionForCollection("Program element types", ProgramElementTypes),
                    GetDescriptionForCollection("Access levels", AccessLevels),
                    GetDescriptionForCollection("Search terms", SearchTerms)
                };
        }

        private static string GetDescriptionForCollection(string collectionName, SortedSet<string> collection)
        {
            return collection.Any() ? String.Format("{0}:[{1}]", collectionName,  String.Join(",", collection)) : null;
        }
    }
}