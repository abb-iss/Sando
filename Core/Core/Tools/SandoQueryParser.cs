using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sando.Core.Tools
{
    public class SandoQueryParser
    {
        private const string LocationFilterRegex = @"(?<filter>\-?location:(?<location>(""[\w\:\.\\\(\)\[\]\{\}\* ]+"")|[\w\:\.\\\(\)\[\]\{\}\*]+))";
        private const string LiteralSearchRegex = @"(?<literal>\-?""(?<literalcontent>[^""\\]*(?:\\.[^""\\]*)*)"")";
        private const string FileExtensionFilterRegex = @"(?<filter>\-?file:\.?(?<fileext>\w+))";
        private const string ProgramElementTypeFilterRegex = @"(?<filter>\-?type:(?<type>field|method|property|enum|struct|class))";
        private const string AccessLevelFilterRegex = @"(?<filter>\-?access:(?<access>public|private|protected|internal))";
        private const string InvalidCharactersRegex = "[^a-zA-Z0-9_\\s\\*\\-]";

        public SandoQueryDescription Parse(string query)
        {
            var sandoQueryDescription = new SandoQueryDescription {OriginalQuery = query};
            var parseFunctions = GetQueryParseFunctions();
            var analyzedQuery = query;
            foreach (var function in parseFunctions)
            {
                if (String.IsNullOrWhiteSpace(analyzedQuery))
                    return sandoQueryDescription;
                analyzedQuery = function(analyzedQuery, sandoQueryDescription);
            }
            return sandoQueryDescription;
        }

        private static IEnumerable<Func<string, SandoQueryDescription, string>> GetQueryParseFunctions()
        {
            return new List<Func<string, SandoQueryDescription, string>>
                {
                    ParseLocationFilters,
                    ParseLiteralSearchTerms,
                    ParseFileExtensionFilters,
                    ParseProgramElementTypeFilters,
                    ParseAccessLevelFilters,
                    ParseNormalSearchTerms
                };
        }

        private static string ParseLocationFilters(string query, SandoQueryDescription sandoQueryDescription)
        {
            var matches = Regex.Matches(query, LocationFilterRegex, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var matchedFilter = match.Groups["filter"].Value;
                var matchedLocation = match.Groups["location"].Value;
                if (!String.IsNullOrWhiteSpace(matchedLocation))
                {
                    sandoQueryDescription.Locations.Add(matchedLocation);
                }
                query = query.Replace(matchedFilter, " ");
            }
            return query;
        }

        private static string ParseLiteralSearchTerms(string query, SandoQueryDescription sandoQueryDescription)
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
                query = query.Replace(matchedLiteral, " ");
            }
            query = query.Replace("\"", String.Empty);
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
            var wordSplitter = new WordSplitter();
            
            //add unsplit words
            var splitTerms = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string term in splitTerms)
            {
                sandoQueryDescription.SearchTerms.Add(term.Trim());           
            }

            var searchTerms = wordSplitter.ExtractWords(query).Where(w => !String.IsNullOrWhiteSpace(w));
            foreach (var searchTerm in searchTerms)
            {
                sandoQueryDescription.SearchTerms.Add(searchTerm.Trim());
                query = query.Replace(searchTerm, " ");
            }
            sandoQueryDescription.SearchTerms = sandoQueryDescription.SearchTerms.Distinct().ToList();
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
            Locations = new List<string>();
            AccessLevels = new List<string>();
        }

        public string OriginalQuery { get; set; }
        public List<string> SearchTerms { get; set; }
        public List<string> LiteralSearchTerms { get; set; }
        public List<string> FileExtensions { get; set; }
        public List<string> ProgramElementTypes { get; set; }
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