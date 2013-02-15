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
using ABB.SrcML.VisualStudio.SolutionMonitor;
using Sando.Core.Tools;

namespace Sando.Indexer.IndexFiltering
{
    public class IndexFilterManager : IIndexFilterManager
    {
        protected IndexFilterSettings IndexFilterSettings { get; private set; }

        protected ILog Logger { get; set; }

        private const string IndexFilterSettingsFileName = ".sandoignore.xml";
        private const string IndexFilterSettingsLogFileName = ".sandoignore.log";
        private string IndexFilterSettingsFilePath;

        public IndexFilterManager()
        {
            var solutionKey = ServiceLocator.Resolve<SolutionKey>();
            IndexFilterSettingsFilePath = Path.Combine(PathManager.Instance.GetIndexPath(solutionKey), IndexFilterSettingsFileName);
            IndexFilterSettings = File.Exists(IndexFilterSettingsFilePath) ? GetIndexFilterSettingsFromFile(IndexFilterSettingsFilePath) : GetDefaultIndexFilterSettings();
            Logger = FileLogger.CreateFileLogger("IndexFilterManagerLogger", Path.Combine(PathManager.Instance.GetIndexPath(solutionKey), IndexFilterSettingsLogFileName));
        }

        public void Dispose()
        {
            SaveIndexFilterSettingsToFile(IndexFilterSettings, IndexFilterSettingsFilePath);
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

        private static void SaveIndexFilterSettingsToFile(IndexFilterSettings currentFilterSettings, string indexStatePath)
        {
            if (currentFilterSettings == null)
                return;

            XmlSerializer xmlSerializer = null;
            TextWriter textWriter = null;
            try
            {
                xmlSerializer = new XmlSerializer(typeof(IndexFilterSettings));
                textWriter = new StreamWriter(indexStatePath);
                xmlSerializer.Serialize(textWriter, currentFilterSettings);
            }
            finally
            {
                if (textWriter != null)
                    textWriter.Close();
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
                            "obj",
                            ".hg"
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