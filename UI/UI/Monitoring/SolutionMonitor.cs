using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sando.Core;
using Sando.Core.Extensions;
using Sando.Indexer;
using Thread = System.Threading.Thread;

namespace Sando.UI.Monitoring
{
	public class SolutionMonitor : IVsRunningDocTableEvents
	{
        private readonly SolutionWrapper _openSolution;
		private DocumentIndexer _currentIndexer;
		private IVsRunningDocumentTable _documentTable;
		private uint _documentTableItemId;
		
		private readonly string _currentPath;		
		private readonly System.ComponentModel.BackgroundWorker _processFileInBackground;
		private readonly SolutionKey _solutionKey;
		private Thread _startupThread;

        public bool PerformingInitialIndexing()
        {
            return !_initialIndexDone;
        }

		private readonly IndexUpdateManager _indexUpdateManager;
	    private bool _initialIndexDone = false;

		public SolutionMonitor(SolutionWrapper openSolution, SolutionKey solutionKey, DocumentIndexer currentIndexer, bool isIndexRecreationRequired)
		{
			_openSolution = openSolution;
			_currentIndexer = currentIndexer;
			_currentPath = solutionKey.GetIndexPath();			
			_solutionKey = solutionKey;
			_indexUpdateManager = new IndexUpdateManager(solutionKey, _currentIndexer, isIndexRecreationRequired);

			_processFileInBackground = new System.ComponentModel.BackgroundWorker();
			_processFileInBackground.DoWork +=
				new DoWorkEventHandler(_processFileInBackground_DoWork);		
		}

		private void _processFileInBackground_DoWork(object sender, DoWorkEventArgs e)
		{
			ProjectItem projectItem = e.Argument as ProjectItem;
			ProcessItem(projectItem);			
            UpdateAfterAdditions();
		}

		private void _runStartupInBackground_DoWork()
		{
			var allProjects = _openSolution.getProjects();
			var enumerator = allProjects.GetEnumerator();
			while(enumerator.MoveNext())
			{
				var project = (Project)enumerator.Current;
                if (project != null)
                {
                    if (project.ProjectItems != null)
                    {
                        ProcessItems(project.ProjectItems.GetEnumerator());
                        UpdateAfterAdditions();
                    }
                }
			}
		    _initialIndexDone = true;
		}

	    public void UpdateAfterAdditions()
	    {
	        _currentIndexer.CommitChanges();
	        _indexUpdateManager.SaveFileStates();
	    }

	    public void StartMonitoring()
		{
			
            _startupThread = new System.Threading.Thread(new ThreadStart(_runStartupInBackground_DoWork));
            _startupThread.Priority = ThreadPriority.Lowest;                 
            _startupThread.Start();

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
            {
                try
                {
                    ProcessItems(item.ProjectItems.GetEnumerator());
                }catch(NullReferenceException nre)
                {
                    //item.ProjectItems == null during shutdown
                    //thus, ignore this error, as it only occurs during shutdown
                    Debug.WriteLine(nre.StackTrace);
                }
            }
		}

		private void ProcessSingleFile(ProjectItem item)
		{
		    try
		    {
		        if (item != null && item.Name != null)
		        {
		            string path = item.FileNames[0];
		            string fileExtension = Path.GetExtension(path);
		            if (fileExtension != null && !fileExtension.Equals(String.Empty))
		            {
		                if (ExtensionPointsRepository.Instance.GetParserImplementation(fileExtension) != null)
		                {
		                    Debug.WriteLine("Start: " + path);

		                    ProcessFileForTesting(path);
		                    Debug.WriteLine("End: " + path);
		                }
		            }
		        }
		    }
		        //TODO - don't catch a generic exception
		    catch (Exception e)
		    {
		        Debug.WriteLine(e.StackTrace);
		    }
		}

	    public void ProcessFileForTesting(string path)
	    {
	        _indexUpdateManager.UpdateFile(path);
	    }

        public void StopMonitoring()
        {
            Dispose();
        }

	    public void Dispose()
		{
            try
            {
                if (_documentTable != null)
                {
                    _documentTable.UnadviseRunningDocTableEvents(_documentTableItemId);
                }

                //shut down any current indexing from the startup thread
                if (_startupThread != null)
                {
                    _startupThread.Abort();
                }
            }
            finally
            {
                //shut down the current indexer
                if (_currentIndexer != null)
                {
                    //cleanup 
                    _currentIndexer.CommitChanges();
                    _indexUpdateManager.SaveFileStates();
                    //dispose
                    _currentIndexer.Dispose();
                    _currentIndexer = null;
                }
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
}
