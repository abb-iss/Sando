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
            this.analyzer.StartAnalysis();
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
                return true;
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