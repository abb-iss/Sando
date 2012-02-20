using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sando.Core;
using Sando.Indexer;
using Sando.Indexer.Documents;
using Microsoft.VisualStudio;
using Sando.Parser;

namespace Sando.UI
{

	class SolutionMonitor : IVsRunningDocTableEvents
	{
		private readonly Solution _openSolution;
		private DocumentIndexer _currentIndexer;
		private IVsRunningDocumentTable _documentTable;
		private uint _documentTableItemId;
		private readonly ParserInterface _parser = new SrcMLParser();
		private string _currentPath;
		private System.ComponentModel.BackgroundWorker _runStartupInBackground;
		private System.ComponentModel.BackgroundWorker _processFileInBackground;
		private SolutionKey _solutionKey;		

		public SolutionMonitor(Solution openSolution, SolutionKey solutionKey, DocumentIndexer currentIndexer)
		{
			this._openSolution = openSolution;
			this._currentIndexer = currentIndexer;
			this._currentPath = solutionKey.GetIndexPath();
			
			_solutionKey = solutionKey;

			_runStartupInBackground = new System.ComponentModel.BackgroundWorker();		
			_runStartupInBackground.DoWork +=
				new DoWorkEventHandler(_runStartupInBackground_DoWork);

			_processFileInBackground = new System.ComponentModel.BackgroundWorker();
			_processFileInBackground.DoWork +=
				new DoWorkEventHandler(_processFileInBackground_DoWork);			
		}

		private void _processFileInBackground_DoWork(object sender, DoWorkEventArgs e)
		{
			ProjectItem projectItem = e.Argument as ProjectItem;
			ProcessItem(projectItem);
			_currentIndexer.CommitChanges();

		}

		private void _runStartupInBackground_DoWork(object sender,
			DoWorkEventArgs e)
		{
			var allProjects = _openSolution.Projects;
			var enumerator = allProjects.GetEnumerator();
			while(enumerator.MoveNext())
			{
				var project = (Project)enumerator.Current;
				ProcessItems(project.ProjectItems.GetEnumerator());
				_currentIndexer.CommitChanges();
			}			
		}

		public void StartMonitoring()
		{

			_runStartupInBackground.RunWorkerAsync();

			// Register events for doc table
			_documentTable = (IVsRunningDocumentTable)Package.GetGlobalService(typeof(SVsRunningDocumentTable));
			_documentTable.AdviseRunningDocTableEvents(this, out _documentTableItemId);
		}

		private void ProcessItems(IEnumerator items)
		{			
			while (items.MoveNext())
			{
				var item = (ProjectItem) items.Current;
				ProcessItem(item);
			}
		}

		private void ProcessItem(ProjectItem item)
		{
			ProcessSingleFile(item);
			ProcessChildren(item);
		}

		private void ProcessChildren(ProjectItem item)
		{
			if (item.ProjectItems != null)
				ProcessItems(item.ProjectItems.GetEnumerator());
		}

		private void ProcessSingleFile(ProjectItem item)
		{
			Debug.WriteLine("processed: " + item.Name);
			if (item.Name.EndsWith(".cs"))
			{
				try
				{
					var path = item.FileNames[0];
					var parsed = _parser.Parse(path);
					foreach (var programElement in parsed)
					{
						var document = DocumentFactory.Create(programElement);
						if(document !=null)
							_currentIndexer.AddDocument(document);
					}
				}
				catch (ArgumentException argumentException)
				{
					//ignore items with no associated file
				}catch(XmlException xmlException)
				{
					//TODO - should fix this if it happens too often
					//TODO - need to investigate why this is happening during parsing
					Debug.WriteLine(xmlException);
				}catch(NullReferenceException nre)
				{
					//TODO - these need to be handled
					//TODO - need to investigate why this is happening during parsing
					Debug.WriteLine(nre);
				}
			}
		}

		public void Dispose()
		{
			_documentTable.UnadviseRunningDocTableEvents(_documentTableItemId);
			
			//shut down the current indexer
			if(_currentIndexer != null)
			{
				_currentIndexer.Dispose();
				_currentIndexer = null;
			}
		}

		public int OnAfterFirstDocumentLock(uint cookie, uint lockType, uint readLocksLeft, uint editLocksLeft)
		{
			//throw new NotImplementedException();
			return VSConstants.S_OK;
		}

		public int OnBeforeLastDocumentUnlock(uint cookie, uint lockType, uint readLocksLeft, uint editLocksLeft)
		{
			//throw new NotImplementedException();
			return VSConstants.S_OK;
		}

		public int OnAfterSave(uint cookie)
		{
			uint  readingLocks, edittingLocks, flags; IVsHierarchy hierarchy; IntPtr documentData; string name;	 uint documentId;
			_documentTable.GetDocumentInfo(cookie, out flags, out readingLocks, out edittingLocks, out name, out hierarchy, out documentId, out documentData);
			var projectItem = _openSolution.FindProjectItem(name);
			if(projectItem!=null)
			{
				_processFileInBackground.RunWorkerAsync(projectItem);
			}
			return VSConstants.S_OK;
		}

		public int OnAfterAttributeChange(uint cookie, uint grfAttribs)
		{
			return VSConstants.S_OK;
		}

		public int OnBeforeDocumentWindowShow(uint cookie, int fFirstShow, IVsWindowFrame pFrame)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
		{
			return VSConstants.S_OK;
		}


		public string GetCurrentDirectory()
		{
			return _currentPath;
		}

		public SolutionKey GetSolutionKey()
		{
			return _solutionKey;
		}


		public void AddUpdateListener(IIndexUpdateListener listener)
		{
			_currentIndexer.AddIndexUpdateListener(listener);
		}

		public void RemoveUpdateListener(IIndexUpdateListener listener)
		{
			_currentIndexer.RemoveIndexUpdateListener(listener);
		}
	}

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
