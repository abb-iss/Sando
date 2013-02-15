using System;
using System.Globalization;
using System.IO;

namespace Sando.Core.Tools
{
    public static class LuceneDirectoryHelper
    {
        public static string GetOrCreateLuceneDirectoryForSolution(string solutionFullName, string luceneDirectoryParentPath)
        {
            if (String.IsNullOrWhiteSpace(solutionFullName) || String.IsNullOrWhiteSpace(luceneDirectoryParentPath) || !Directory.Exists(luceneDirectoryParentPath))
                return String.Empty;

            var solutionName = Path.GetFileName(solutionFullName) ?? Guid.NewGuid().ToString();
            var solutionNameHash = solutionName.GetHashCode();

            var luceneDirectoryPath = Path.Combine(luceneDirectoryParentPath, LuceneDirectoryName, solutionNameHash.ToString(CultureInfo.InvariantCulture));
            if (!Directory.Exists(luceneDirectoryPath)) 
                Directory.CreateDirectory(luceneDirectoryPath);
            return luceneDirectoryPath;
        }

        public static string GetOrCreateSrcMlArchivesDirectoryForSolution(string solutionFullName, string luceneDirectoryParentPath)
        {
            if (String.IsNullOrWhiteSpace(solutionFullName) || String.IsNullOrWhiteSpace(luceneDirectoryParentPath) || !File.Exists(solutionFullName) || !Directory.Exists(luceneDirectoryParentPath))
                return String.Empty;

            var solutionName = Path.GetFileName(solutionFullName) ?? Guid.NewGuid().ToString();
            var solutionNameHash = solutionName.GetHashCode();

            var srvMlDirectoryPath = Path.Combine(luceneDirectoryParentPath, SrcMlDirectoryName, solutionNameHash.ToString(CultureInfo.InvariantCulture));
            if (!Directory.Exists(srvMlDirectoryPath))
                Directory.CreateDirectory(srvMlDirectoryPath);
            return srvMlDirectoryPath;
        }

        private const string LuceneDirectoryName = "lucene";
        private const string SrcMlDirectoryName = "srcMlArchives";
    }
}