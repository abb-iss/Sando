using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sando.Core.Tools
{
    public class SandoQueryParser
    {
        private const string LiteralSearchRegex = @"(?<literal>\-?""(?<literalcontent>[^""\\]*(?:\\.[^""\\]*)*)"")";
        private const string FileExtensionFilterRegex = @"(?<filter>\-?file:(?<fileext>\w+))";
        private const string ProgramElementTypeFilterRegex = @"(?<filter>\-?type:(?<type>field|method|property|enum|struct|class))";
        private const string AccessLevelFilterRegex = @"(?<filter>\-?access:(?<access>public|private|protected|internal))";

        public SandoQueryDescription Parse(string query)
        {
            var sandoQueryDescription = new SandoQueryDescription();
            var parseFunctions = GetQueryParseFunctions();
            foreach (var function in parseFunctions)
            {
                if (String.IsNullOrWhiteSpace(query))
                    return sandoQueryDescription;
                query = function(query, sandoQueryDescription);
            }
            return sandoQueryDescription;
        }

        private static IEnumerable<Func<string, SandoQueryDescription, string>> GetQueryParseFunctions()
        {
            return new List<Func<string, SandoQueryDescription, string>>
                {
                    ParseLiteral,
                    ParseFileExtensionFilters,
                    ParseProgramElementTypeFilters,
                    ParseAccessLevelFilters
                };
        }

        private static string ParseLiteral(string query, SandoQueryDescription sandoQueryDescription)
        {
            var matches = Regex.Matches(query, LiteralSearchRegex);
            foreach (Match match in matches)
            {
                var matchedLiteral = match.Groups["literal"].Value;
                var matchedLiteralContent = match.Groups["literalcontent"].Value;
                if (!String.IsNullOrWhiteSpace(matchedLiteralContent))
                {
                    sandoQueryDescription.LiteralSearchTerms.Add(matchedLiteral);
                }
                query = query.Replace(matchedLiteral, String.Empty);
            }
            query = query.Replace("\"", String.Empty);
            return query;
        }

        private static string ParseFileExtensionFilters(string query, SandoQueryDescription sandoQueryDescription)
        {
            var matches = Regex.Matches(query, FileExtensionFilterRegex);
            foreach (Match match in matches)
            {
                var matchedFilter = match.Groups["filter"].Value;
                var matchedFileExtension = match.Groups["fileext"].Value;
                if (matchedFilter.StartsWith("-"))
                    sandoQueryDescription.FileExtensions.Add("-" + matchedFileExtension);
                else
                    sandoQueryDescription.FileExtensions.Add(matchedFileExtension);
                query = query.Replace(matchedFilter, String.Empty);
            }
            query = query.Replace("\"", String.Empty);
            return query;
        }

        private static string ParseProgramElementTypeFilters(string query, SandoQueryDescription sandoQueryDescription)
        {
            var matches = Regex.Matches(query, ProgramElementTypeFilterRegex);
            foreach (Match match in matches)
            {
                var matchedFilter = match.Groups["filter"].Value;
                var matchedProgramElementType = match.Groups["type"].Value;
                if (matchedFilter.StartsWith("-"))
                    sandoQueryDescription.ProgramElementTypes.Add("-" + matchedProgramElementType);
                else
                    sandoQueryDescription.ProgramElementTypes.Add(matchedProgramElementType);
                query = query.Replace(matchedFilter, String.Empty);
            }
            query = query.Replace("\"", String.Empty);
            return query;
        }

        private static string ParseAccessLevelFilters(string query, SandoQueryDescription sandoQueryDescription)
        {
            var matches = Regex.Matches(query, AccessLevelFilterRegex);
            foreach (Match match in matches)
            {
                var matchedFilter = match.Groups["filter"].Value;
                var matchedAccessLevel = match.Groups["access"].Value;
                if (matchedFilter.StartsWith("-"))
                    sandoQueryDescription.AccessLevels.Add("-" + matchedAccessLevel);
                else
                    sandoQueryDescription.AccessLevels.Add(matchedAccessLevel);
                query = query.Replace(matchedFilter, String.Empty);
            }
            query = query.Replace("\"", String.Empty);
            return query;
        }
    }

    public class SandoQueryDescription
    {
        public SandoQueryDescription()
        {
            SearchTerms = new List<string>();
            LiteralSearchTerms = new List<string>();
            FileExtensions = new List<string>();
            ProgramElementTypes = new List<string>();
            WildcardSearchTerms = new List<string>();
            Locations = new List<string>();
            AccessLevels = new List<string>();
        }

        public List<string> SearchTerms { get; set; }
        public List<string> LiteralSearchTerms { get; set; }
        public List<string> FileExtensions { get; set; }
        public List<string> ProgramElementTypes { get; set; }
        public List<string> WildcardSearchTerms { get; set; }
        public List<string> Locations { get; set; }
        public List<string> AccessLevels { get; set; }

        public bool IsValid
        {
            get
            {
                return SearchTerms.Any() ||
                       LiteralSearchTerms.Any() ||
                       FileExtensions.Any() ||
                       ProgramElementTypes.Any() ||
                       WildcardSearchTerms.Any() ||
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
                    GetDescriptionForCollection("Search terms", SearchTerms),
                    GetDescriptionForCollection("Literal search terms", LiteralSearchTerms),
                    GetDescriptionForCollection("File extensions", FileExtensions),
                    GetDescriptionForCollection("Program element types", ProgramElementTypes),
                    GetDescriptionForCollection("Wildcard search terms", WildcardSearchTerms),
                    GetDescriptionForCollection("Locations", Locations),
                    GetDescriptionForCollection("Access levels", AccessLevels)
                };
        }

        private static string GetDescriptionForCollection(string collectionName, List<string> collection)
        {
            return collection.Any() ? String.Format("{0}:[{1}]", collectionName,  String.Join(",", collection)) : null;
        }
    }
}