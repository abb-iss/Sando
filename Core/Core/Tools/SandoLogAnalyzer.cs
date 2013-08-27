using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sando.Core.Tools
{
    public interface ILogFileAnalyzer
    {
        void StartAnalyze(ILogFile file);
        void FinishAnalysis();
    }

    public interface ILogFile
    {
        String Name { get; }
        String Content { get; }
    }

    public class SandoAnalysisManager
    {
        private readonly SandoLogAnalyzer analyzer;

        public SandoAnalysisManager(string directory)
        {
            this.analyzer = new SandoLogAnalyzer(directory);
        }

        public void Analyze()
        {
            this.analyzer.AddAnalyzer(new NoSearchResultsAnalyzer());
            this.analyzer.AddAnalyzer(new NumberOfUsersAnalyzer());
            this.analyzer.AddAnalyzer(new PreSearchRecommendationAnalyzer());
            this.analyzer.AddAnalyzer(new PostSearchRecommendationAnalyzer());
            this.analyzer.AddAnalyzer(new ClickPostSearchRecommendationAnalyzer());
            this.analyzer.AddAnalyzer(new TagCloudAnalyzer());
            this.analyzer.AddAnalyzer(new SelectTagAnalyzer());
            this.analyzer.StartAnalysis();
        }


        private static void WriteToResult(String s)
        {
            using (StreamWriter writer = File.AppendText(@"C:\study data\Sando\results.txt"))
            {
                writer.WriteLine(s);
            }
        }
        
        private class PreSearchRecommendationAnalyzer : ILogFileAnalyzer
        {
            private const String start = "Pre-search recommendations";
            private const String start2 = "Recommendation item selected";
            private int VDCount = 0;
            private int VariableCount = 0;
            private int selected = 0;

            public void StartAnalyze(ILogFile file)
            {
                var lines = file.Content.Split('\n');
                lines = lines.Where(l => l.Contains(start) || l.Contains(start2)).ToArray();
                selected += lines.Count();
            }

            public void FinishAnalysis()
            {
                WriteToResult("selected pre-search recommendations:" + selected);
            }
        }

        private class TagCloudAnalyzer : ILogFileAnalyzer
        {
            private const String start = "TagCloud: Render a tag cloud.";
            private int count = 0;

            public void StartAnalyze(ILogFile file)
            {
                var lines = file.Content.Split('\n');
                lines = lines.Where(l => l.Contains(start)).ToArray();
                count += lines.Count();
            }

            public void FinishAnalysis()
            {
                WriteToResult("Showing tag cloud:" + count);
            }
        }

        private class SelectTagAnalyzer : ILogFileAnalyzer
        {
            private const String start = "Select a tag.";
            private int count = 0;

            public void StartAnalyze(ILogFile file)
            {
                var lines = file.Content.Split('\n');
                lines = lines.Where(l => l.Contains(start)).ToArray();
                count += lines.Count();
            }

            public void FinishAnalysis()
            {
                WriteToResult("Selecting a tag:" + count);
            }
        }



        private class PostSearchRecommendationAnalyzer : ILogFileAnalyzer
        {
            private const String start = "Create links";
            private const String start2 = "IReformedQuery: Issue reformed queries";
            private int count = 0;
           

            public void StartAnalyze(ILogFile file)
            {
                var lines = file.Content.Split('\n');
                lines = lines.Where(l => l.Contains(start) || l.Contains(start2)).ToArray();
                count += lines.Count();
            }

            public void FinishAnalysis()
            {
                WriteToResult("Post recommendation issued:" + count);
            }
        }

        private class ClickPostSearchRecommendationAnalyzer : ILogFileAnalyzer
        {
            private const String start = "Clicked link:";
            private const String start2 = "IReformedQuery: Selected recommendation";

            private int count = 0;


            public void StartAnalyze(ILogFile file)
            {
                var lines = file.Content.Split('\n');
                lines = lines.Where(l => l.Contains(start) || l.Contains(start2)).ToArray();
                count += lines.Count();
            }

            public void FinishAnalysis()
            {
                WriteToResult("Clicking a post-search recommendation:" + count);
            }
        }



        private class NoSearchResultsAnalyzer : ILogFileAnalyzer
        {
            private int allQueryCount = 0;
            private int emptyQueryCount = 0;

            public void StartAnalyze(ILogFile file)
            {
                var lines = file.Content.Split('\n');
                lines = lines.Where(l => l.Contains("Sando returned results")).ToArray();
                allQueryCount += lines.Count();
                emptyQueryCount += lines.Count(l => l.Contains("NumberOfResults=0"));
            }

            public void FinishAnalysis()
            {
                var sb = new StringBuilder();
                sb.AppendLine("All query count: " + allQueryCount);
                sb.AppendLine("No result query count: " + emptyQueryCount);
                string result = sb.ToString();
                WriteToResult(result);
            }
        }

        private class NumberOfUsersAnalyzer : ILogFileAnalyzer
        {
            private readonly Dictionary<string, int> IDs = new Dictionary<string, int>();  

            public void StartAnalyze(ILogFile file)
            {
                var id = file.Name.Split('_')[2];
                if (IDs.ContainsKey(id))
                {
                    IDs[id]++;
                }
                else
                {
                    IDs.Add(id, 1);
                }
            }

            public void FinishAnalysis()
            {
                int count = IDs.Keys.Count;
                WriteToResult("Number of users:" + count);
            }
        }



        public class SandoLogAnalyzer
        {
            private readonly string directory;
            private readonly List<ILogFileAnalyzer> analyzers;

            public SandoLogAnalyzer(string directory)
            {
                this.directory = directory;
                this.analyzers = new List<ILogFileAnalyzer>();
            }

            private class LogFile : ILogFile
            {
                public String Name { private set; get; }
                public String Content { private set; get; }

                public LogFile(String Name, String Content)
                {
                    this.Name = Name;
                    this.Content = Content;
                }
            }

            private LogFile[] GetLogFiles()
            {
                var files = new List<LogFile>();
                var dir = new DirectoryInfo(directory);

                foreach (FileInfo file in dir.GetFiles("*.log"))
                {
                    var path = file.Name;
                    if (IsFileNameGood(path))
                    {
                        using (var reader = file.OpenText())
                        {
                            var content = reader.ReadToEnd();
                            files.Add(new LogFile(path, content));
                        }
                    }
                }
                return files.ToArray();
            }

            private bool IsFileNameGood(String fileName)
            {
                var shepherd = "2021486822";
                var xi = "1914121570";

                var list = new List<String> {shepherd, xi};
                return list.All(l => !fileName.Contains(l));
            }


            public void AddAnalyzer(ILogFileAnalyzer analyzer)
            {
                this.analyzers.Add(analyzer);
            }

            public void StartAnalysis()
            {
                var logFiles = GetLogFiles();
                foreach (var analyzer in analyzers)
                {
                    foreach (var log in logFiles)
                    {
                        analyzer.StartAnalyze(log);
                    }
                    analyzer.FinishAnalysis();
                }
            }
        }
    }
}