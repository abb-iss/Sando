using System;
using System.Globalization;
using System.IO;

namespace Sando.Core.Tools
{
    public static class LuceneDirectoryHelper
    {
        public static string GetOrCreateLuceneDirectoryForSolution(string solutionFullName, string luceneDirectoryParentPath)
        {
            if (String.IsNullOrWhiteSpace(solutionFullName) || String.IsNullOrWhiteSpace(luceneDirectoryParentPath) || !File.Exists(solutionFullName) || !Directory.Exists(luceneDirectoryParentPath))
                return String.Empty;

            var solutionName = Path.GetFileName(solutionFullName) ?? Guid.NewGuid().ToString();
            var solutionNameHash = solutionName.GetHashCode();

            var luceneFolderPath = Path.Combine(luceneDirectoryParentPath, LuceneDirectoryName, solutionNameHash.ToString(CultureInfo.InvariantCulture));
            if (!Directory.Exists(luceneFolderPath)) 
                Directory.CreateDirectory(luceneFolderPath);
            return luceneFolderPath;
        }

        public static string GetOrCreateSrcMlArchivesDirectoryForSolution(string solutionFullName, string luceneDirectoryParentPath)
        {
            if (!File.Exists(solutionFullName) || String.IsNullOrWhiteSpace(luceneDirectoryParentPath))
                return String.Empty;

            var solutionName = Path.GetFileName(solutionFullName) ?? Guid.NewGuid().ToString();
            var solutionNameHash = solutionName.GetHashCode();

            var luceneFolderPath = Path.Combine(luceneDirectoryParentPath, SrcMlDirectoryName, solutionNameHash.ToString(CultureInfo.InvariantCulture));
            if (!Directory.Exists(luceneFolderPath))
                Directory.CreateDirectory(luceneFolderPath);
            return luceneFolderPath;
        }

        private const string LuceneDirectoryName = "lucene";
        private const string SrcMlDirectoryName = "srcMlArchives";
    }
}