using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Sando.Core;
using Sando.Core.Extensions.Logging;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.IndexerContracts;
using System.Linq;
using log4net;

namespace Sando.Indexer.IndexFiltering
{
    public class IndexFilterManager : IIndexFilterManager
    {
        protected IndexFilterSettings IndexFilterSettings { get; private set; }

        protected ILog Logger { get; set; }

        private const string IndexFilterSettingsFileName = ".sandoignore.xml";
        private const string IndexFilterSettingsLogFileName = ".sandoignore.log";

        public IndexFilterManager()
        {
            var solutionKey = ServiceLocator.Resolve<SolutionKey>();
            var indexFilterSettingsFilePath = Path.Combine(solutionKey.IndexPath, IndexFilterSettingsFileName);
            IndexFilterSettings = File.Exists(indexFilterSettingsFilePath) ? GetIndexFilterSettingsFromFile(indexFilterSettingsFilePath) : GetDefaultIndexFilterSettings();
            Logger = FileLogger.CreateFileLogger("IndexFilterManagerLogger", Path.Combine(solutionKey.IndexPath, IndexFilterSettingsLogFileName));
        }

        public IndexFilterManager(IndexFilterSettings indexFilterSettings, ILog logger)
        {
            Contract.Requires(indexFilterSettings != null, "IndexFilterManager:Constructor - index filter settings cannot be null!");
            Contract.Requires(logger != null, "IndexFilterManager:Constructor - logger cannot be null!");

            IndexFilterSettings = indexFilterSettings;
            Logger = logger;
        }

        public bool ShouldFileBeIndexed(string fullFilePath)
        {
            if (String.IsNullOrWhiteSpace(fullFilePath))
                return false;
            if (File.Exists(fullFilePath))
            {
                var fileInfo = new FileInfo(fullFilePath);

                if (IndexFilterSettings.IgnoredExtensions.Contains(fileInfo.Extension))
                {
                    Logger.Info(GetRuleCode(IndexFilterRuleCode.IgnoredExtensions, fileInfo.Extension));
                    return false;
                }

                if (IndexFilterSettings.IgnoredFileNames.Contains(fileInfo.Name))
                {
                    Logger.Info(GetRuleCode(IndexFilterRuleCode.IgnoredFileNames, fileInfo.Name));
                    return false;
                }

                var directoryInfo = fileInfo.Directory;
                while (directoryInfo != null)
                {
                    if (IndexFilterSettings.IgnoredDirectories.Contains(directoryInfo.Name))
                    {
                        Logger.Info(GetRuleCode(IndexFilterRuleCode.IgnoredDirectories, directoryInfo.Name));
                        return false;
                    }
                    directoryInfo = directoryInfo.Parent;
                }

                var invalidPathCharacters = Path.GetInvalidPathChars().ToList();
                invalidPathCharacters.Remove('*');
                foreach (var ignoredPathExpression in IndexFilterSettings.IgnoredPathExpressions)
                {
                    if (invalidPathCharacters.Any(ignoredPathExpression.Contains))
                    {
                        Logger.WarnFormat("Invalid path expression: {0}", ignoredPathExpression);
                        continue;
                    }
                    if (PathMatches(fileInfo.FullName, ignoredPathExpression))
                    {
                        Logger.Info(GetRuleCode(IndexFilterRuleCode.IgnoredPathExpressions, ignoredPathExpression));
                        return false;
                    }
                }

                foreach (var ignoredPathRegularExpression in IndexFilterSettings.IgnoredPathRegularExpressions)
                {
                    try
                    {
                        if (Regex.IsMatch(fileInfo.FullName, ignoredPathRegularExpression))
                        {
                            Logger.Info(GetRuleCode(IndexFilterRuleCode.IgnoredPathRegularExpressions, ignoredPathRegularExpression));
                            return false;
                        }
                    }
                    catch (ArgumentException e)
                    {
                        Logger.WarnFormat("Invalid path regular expression: {0}", ignoredPathRegularExpression);
                    }
                }
            }
            return true;
        }

        private static IndexFilterSettings GetIndexFilterSettingsFromFile(string indexFilterSettingsFilePath)
        {
            TextReader textReader = null;
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(IndexFilterSettings));
                textReader = new StreamReader(indexFilterSettingsFilePath);
                var indexFilterSettings = (IndexFilterSettings) xmlSerializer.Deserialize(textReader);
                return indexFilterSettings;
            }
            finally
            {
                if (textReader != null)
                    textReader.Close();
            }
        }

        private static IndexFilterSettings GetDefaultIndexFilterSettings()
        {
            var indexFilterSettings = new IndexFilterSettings
                {
                    IgnoredExtensions = new List<string>
                        {
                            ".tmp",
                            ".db"
                        },
                    IgnoredDirectories = new List<string>
                        {
                            "bin",
                            "obj"
                        }
                };
            return indexFilterSettings;
        }

        private static bool PathMatches(string fullFilePath, string ignoredPathExpression)
        {
            var pathParts = ignoredPathExpression.Split(new[] { '*', '?' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable();
            pathParts = pathParts.Select(Regex.Escape);
            var regex = String.Join("(.*)", pathParts);
            return Regex.IsMatch(fullFilePath, regex);
        }

        private string GetRuleCode(IndexFilterRuleCode indexFilterRuleCode, string ruleBody)
        {
            return String.Format("{0}: {1}", indexFilterRuleCode, ruleBody);
        }

        private enum IndexFilterRuleCode
        {
            IgnoredExtensions,
            IgnoredFileNames,
            IgnoredDirectories,
            IgnoredPathExpressions,
            IgnoredPathRegularExpressions
        }
    }
}