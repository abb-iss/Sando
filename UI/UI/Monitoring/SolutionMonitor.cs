using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sando.Core;
using Sando.Core.Extensions;
using Sando.Core.Extensions.Logging;
using Sando.Indexer;
using Thread = System.Threading.Thread;

namespace Sando.UI.Monitoring
{
	public class SolutionMonitor : IVsRunningDocTableEvents
	{
	    private const string StartupThreadName = "Sando: Initial Index of Project";
	    private readonly SolutionWrapper _openSolution;
		private DocumentIndexer _currentIndexer;
		private IVsRunningDocumentTable _documentTable;
		private uint _documentTableItemId;
		
		private readonly string _currentPath;		
		private readonly System.ComponentModel.BackgroundWorker _processFileInBackground;
		private readonly SolutionKey _solutionKey;
		private Thread _startupThread;
	    public volatile bool ShouldStop = false;

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
            try
            {
                var allProjects = _openSolution.getProjects();
                var enumerator = allProjects.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var project = (Project)enumerator.Current;
                    if (project != null)
                    {
                        if (project.ProjectItems != null)
                        {
                            try
                            {
                                ProcessItems(project.ProjectItems.GetEnumerator());
                            }
                            finally
                            {
                                UpdateAfterAdditions();
                            }
                        }
                        if (ShouldStop && InStartupThread())
                        {
                            break;
                        }
                    }
                }
            } 
            finally
            {
                _initialIndexDone = true;
                ShouldStop = false;
                Thread.CurrentThread.Interrupt();        
            }            
		}

	    public void UpdateAfterAdditions()
	    {
	        _currentIndexer.CommitChanges();
	        _indexUpdateManager.SaveFileStates();
	    }

	    public void StartMonitoring()
		{
			
            _startupThread = new System.Threading.Thread(new ThreadStart(_runStartupInBackground_DoWork));
	        _startupThread.Name = StartupThreadName;
            _startupThread.Priority = ThreadPriority.BelowNormal;                 
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
                if(ShouldStop && InStartupThread())
                {
                    return;
                }
			}
		}

        private bool InStartupThread()
        {        
            try
            {
        
                Thread current = Thread.CurrentThread;
                if (StartupThreadName.Equals(current.Name))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false; 
            }    
        }

		private void ProcessItem(ProjectItem item)
		{
			ProcessSingleFile(item);
			ProcessChildren(item);
		} 

		private void ProcessChildren(ProjectItem item)
		{
            try
            {
                if(item!=null && item.ProjectItems!=null)
                    ProcessItems(item.ProjectItems.GetEnumerator());
            }catch(COMException dll)
            {
                //ignore, can't parse these types of files
            }
		}

		private void ProcessSingleFile(ProjectItem item)
		{
		    string path = "";
		    try
		    {
		        if (item != null && item.Name != null)
		        {
		            path = item.FileNames[0];
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
                FileLogger.DefaultLogger.Error(ExceptionFormatter.CreateMessage(e, "Problem parsing file: " + path));
		    }
		}

	    public void ProcessFileForTesting(string path)
	    {
	        _indexUpdateManager.UpdateFile(path);
	    }

        public void StopMonitoring(bool killReaders=false)
        {
            Dispose(killReaders);
        }

	    public void Dispose(bool killReaders=false)
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
                    if (_startupThread.IsAlive)
                    {                    
                        ShouldStop = true;
                        _startupThread.Join();
                        //while(_startupThread.ThreadState==ThreadState.Running) 
                        //    Thread.Sleep(500);
                    }
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
                    _currentIndexer.Dispose(killReaders);
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
