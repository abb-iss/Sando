using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sando.Indexer;
using Sando.Indexer.Documents;
using Microsoft.VisualStudio;

namespace Sando.UI
{
	class SolutionMonitor : IVsRunningDocTableEvents
	{
		private Solution openSolution;
		private DocumentIndexer CurrentIndexer;
		private bool Monitoring;
		IVsRunningDocumentTable documentTable;
		uint documentTableItemId = 0;


		public SolutionMonitor(Solution openSolution, DocumentIndexer CurrentIndexer)
		{
			// TODO: Complete member initialization
			this.openSolution = openSolution;
			this.CurrentIndexer = CurrentIndexer;			
			Monitoring = false;
		}

		private void DocumentSaved(Document document)
		{
			if(Monitoring)
			{
				//CurrentIndexer.AddDocument();?			
				Debug.WriteLine("processed: " + document.Name);
			}
		}

		public void StartMonitoring()
		{
				
			var allProjects = openSolution.Projects;
			var enumerator = allProjects.GetEnumerator();
			while (enumerator.MoveNext())
			{
				var project = (Project)enumerator.Current;
				ProcessItems(project.ProjectItems.GetEnumerator());
			}

 
			// Register events for doc table
			documentTable = (IVsRunningDocumentTable)Package.GetGlobalService(typeof(SVsRunningDocumentTable));
			documentTable.AdviseRunningDocTableEvents(this, out documentTableItemId);


			Monitoring = true;
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
			Debug.WriteLine("processed: " + item.Name);
			ProcessItems(item.ProjectItems.GetEnumerator());
		}

		public void Dispose()
		{
			documentTable.UnadviseRunningDocTableEvents(documentTableItemId);
		}

		public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
		{
			//throw new NotImplementedException();
			return VSConstants.S_OK;
		}

		public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
		{
			//throw new NotImplementedException();
			return VSConstants.S_OK;
		}

		public int OnAfterSave(uint docCookie)
		{
			uint  readingLocks, edittingLocks, flags; IVsHierarchy hierarchy; IntPtr documentData; string name;	 uint documentId;
			documentTable.GetDocumentInfo(docCookie, out flags, out readingLocks, out edittingLocks, out name, out hierarchy, out documentId, out documentData);
			var projectItem = openSolution.FindProjectItem(name);
			if(projectItem!=null)
			{
				ProcessItem(projectItem);
			}
			return VSConstants.S_OK;
		}

		public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
		{
			//throw new NotImplementedException();
			return VSConstants.S_OK;
		}

		public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
		{
			//throw new NotImplementedException();
			return VSConstants.S_OK;
		}

		public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
		{
			//throw new NotImplementedException();
			return VSConstants.S_OK;
		}
	}
}
