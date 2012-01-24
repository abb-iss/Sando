using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using Sando.Indexer;

namespace Sando.UI
{
	class SolutionMonitor
	{
		private Solution openSolution;
		private DocumentIndexer CurrentIndexer;

		public SolutionMonitor(Solution openSolution, DocumentIndexer CurrentIndexer)
		{
			this.openSolution = openSolution;
			this.CurrentIndexer = CurrentIndexer;
		}

		public void StartMonitoring()
		{
			
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}
