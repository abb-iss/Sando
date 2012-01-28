using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using EnvDTE;
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
		private Solution openSolution;
		private DocumentIndexer CurrentIndexer;
		private bool Monitoring;
		private IVsRunningDocumentTable documentTable;
		private uint documentTableItemId = 0;
		private ParserInterface Parser = new SrcMLParser();


		public SolutionMonitor(Solution openSolution, DocumentIndexer CurrentIndexer)
		{
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
			CurrentIndexer.CommitChanges();
 
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
					var parsed = Parser.Parse(path);
					foreach (var programElement in parsed)
					{
						//TODO - only processing methods for now
						if(programElement as MethodElement != null)
						{
							CurrentIndexer.AddDocument(MethodDocument.Create(programElement as MethodElement));
						}	
					}
				}
				catch (ArgumentException e)
				{
					//ignore items with no associated file
				}catch(XmlException p)
				{
					//ignore for now
					//TODO - should fix this if it happens too often
					Debug.WriteLine(p);
				}catch(NullReferenceException nre)
				{
					//TODO - these need to be handled
					Debug.WriteLine(nre);
				}
			}
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
				CurrentIndexer.CommitChanges();
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
