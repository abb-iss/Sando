using System;
using System.Diagnostics.Contracts;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Sando.Core;
using Sando.Indexer;

namespace Sando.UI.Monitoring
{
	class SolutionMonitorFactory
	{
		private const string Lucene = "\\lucene";
		private static readonly string LuceneFolder = CreateLuceneFolder();

		public static SolutionMonitor CreateMonitor()
		{
			var openSolution = GetOpenSolution();
			return CreateMonitor(openSolution);
		}

		private static SolutionMonitor CreateMonitor(Solution openSolution)
		{
			Contract.Requires(openSolution != null, "A solution must be open");

			//TODO if solution is reopen - the guid should be read from file - future change
			SolutionKey solutionKey = new SolutionKey(Guid.NewGuid(), openSolution.FileName, GetLuceneDirectoryForSolution(openSolution));
			var currentIndexer = DocumentIndexerFactory.CreateIndexer(solutionKey,
			                                                          AnalyzerType.Standard);
			var currentMonitor = new SolutionMonitor(openSolution, solutionKey, currentIndexer);
			currentMonitor.StartMonitoring();
			return currentMonitor;
		}

		private static string CreateLuceneFolder()
		{
			var current = Directory.GetCurrentDirectory();			
			return CreateFolder(Lucene, current);
		}

		private static string CreateFolder(string name, string current)
		{
			if (!File.Exists(current + name))
			{
				var directoryInfo = Directory.CreateDirectory(current + name);
				return directoryInfo.FullName;
			}
			else
			{
				return name + Lucene;
			}
		}

		private static string GetName(Solution openSolution)
		{
			var fullName = openSolution.FullName;
			var split = fullName.Split('\\');
			return split[split.Length - 1];
		}

		private static Solution GetOpenSolution()
		{
			var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
			if(dte != null)
			{
				var openSolution = dte.Solution;
				return openSolution;
			}else
			{
				return null;
			}
		}

		private static string GetLuceneDirectoryForSolution(Solution openSolution)
		{
			CreateFolder(GetName(openSolution), LuceneFolder + "\\");
			return LuceneFolder + "\\" + GetName(openSolution);
		}
	}
}