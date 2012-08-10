using System;
using System.Collections.Generic;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace Sando.Core.Extensions.PairedInterleaving
{
	public class PairedInterleavingLogEntry
	{
		public PairedInterleavingLogEntry(int id, List<CodeSearchResult> interleavedResults)
		{
			Id = id;
			InterleavedResults = interleavedResults;
			Count = 0;
		}

		public void CountClick(CodeSearchResult clickedElement)
		{

		}

		public string Serialize()
		{
			return Id + ": " + Count;
		}

		public int Id { get; private set; }
		public List<CodeSearchResult> InterleavedResults { get; private set; }
		public int Count { get; private set; }
	}
}
