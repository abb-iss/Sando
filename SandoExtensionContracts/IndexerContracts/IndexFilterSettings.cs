using System.Collections.Generic;

namespace Sando.ExtensionContracts.IndexerContracts
{
    public class IndexFilterSettings
    {
        public IndexFilterSettings()
        {
            IgnoredExtensions = new List<string>();
            IgnoredFileNames = new List<string>();
            IgnoredDirectories = new List<string>();
            IgnoredPathExpressions = new List<string>();
            IgnoredPathRegularExpressions = new List<string>();
        }

        public List<string> IgnoredExtensions { get; set; }

        public List<string> IgnoredFileNames { get; set; }

        public List<string> IgnoredDirectories { get; set; }

        public List<string> IgnoredPathExpressions { get; set; }

        public List<string> IgnoredPathRegularExpressions { get; set; }
    }
}