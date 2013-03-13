using System;
using System.Globalization;
using System.IO;

namespace Sando.Core.Tools
{
    public static class LuceneDirectoryHelper
    {
        public static string GetOrCreateLuceneDirectoryForSolution(string solutionFullName, string luceneDirectoryParentPath)
        {
            return CreateNewDirectory(solutionFullName, luceneDirectoryParentPath, LuceneDirectoryName);
        }

        public static bool DoesLuceneDirectoryForSolutionExist(string solutionFullName, string luceneDirectoryParentPath)
        {
            string dir = CreateNewDirectory(solutionFullName, luceneDirectoryParentPath, LuceneDirectoryName, false);
            return Directory.Exists(dir);
        }

        private static string CreateNewDirectory(string solutionFullName, string luceneDirectoryParentPath, string folderName, bool create = true)
        {
            if (String.IsNullOrWhiteSpace(solutionFullName) || String.IsNullOrWhiteSpace(luceneDirectoryParentPath) || !Directory.Exists(luceneDirectoryParentPath))
                return String.Empty;

            var solutionName = Path.GetFileNameWithoutExtension(solutionFullName) ?? Guid.NewGuid().ToString();
            var solutionNameHash = solutionFullName.GetHashCode();
            var totalName = solutionName + "-" + solutionNameHash;

            var luceneDirectoryPath = Path.Combine(luceneDirectoryParentPath, folderName, totalName.ToString(CultureInfo.InvariantCulture));
            if (!Directory.Exists(luceneDirectoryPath) && create)
                Directory.CreateDirectory(luceneDirectoryPath);
            return luceneDirectoryPath;
        }

        public static string GetOrCreateSrcMlArchivesDirectoryForSolution(string solutionFullName, string luceneDirectoryParentPath)
        {
            return CreateNewDirectory(solutionFullName, luceneDirectoryParentPath, SrcMlDirectoryName);
        }

        private const string LuceneDirectoryName = "lucene";
        private const string SrcMlDirectoryName = "srcMlArchives";
    }
}