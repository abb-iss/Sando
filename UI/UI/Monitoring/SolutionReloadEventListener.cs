using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Sando.UI.Monitoring
{
	/*
	 *  Adapted from https://github.com/shanselman/RestoreAfterReloadVSIX
	 */

	public class SolutionReloadEventListener : IVsSolutionEvents
	{
		private IVsSolution _solution;
		private uint _solutionEventsCookie;

		public event Action OnQueryUnloadProject;
	  
		public SolutionReloadEventListener()
		{
			InitNullEvents();

			_solution = Package.GetGlobalService(typeof (SVsSolution)) as IVsSolution;

			if (_solution != null)
			{
				_solution.AdviseSolutionEvents(this, out _solutionEventsCookie);
			}
		}

		private void InitNullEvents()
		{
			OnQueryUnloadProject += () => { };
		}

		#region IVsSolutionEvents Members

		int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved)
		{
			return VSConstants.S_OK;
		}

		int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
		{
			return VSConstants.S_OK;
		}

		int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
		{
			return VSConstants.S_OK;
		}

		int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
		{

			return VSConstants.S_OK;
		}

		int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
		{
			return VSConstants.S_OK;
		}

		int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
		{
			return VSConstants.S_OK;
		}

		int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
		{
			return VSConstants.S_OK;
		}

		int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
		{
			return VSConstants.S_OK;
		}

		int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
		{
			return VSConstants.S_OK;
		}

		int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
		{
			OnQueryUnloadProject();
			return VSConstants.S_OK;
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (_solution != null && _solutionEventsCookie != 0)
			{
				GC.SuppressFinalize(this);
				_solution.UnadviseSolutionEvents(_solutionEventsCookie);
				OnQueryUnloadProject = null;
				_solutionEventsCookie = 0;
				_solution = null;
			}
		}

		#endregion

	}

}