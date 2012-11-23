using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Sando.ExtensionContracts.QueryContracts;
using Sando.Indexer.Documents;

namespace Sando.Indexer.Searching
{
	public class QueryWeightsSupplier : IQueryWeightsSupplier
	{
		public Dictionary<string, float> GetQueryWeightsValues()
		{
			Contract.Ensures(Contract.Result<Dictionary<string, float>>() != null, "QueryWeightsSupplier:GetQueryWeightsValues - an object must be returned from this method!");

			Dictionary<string, float> currentWeigths = new Dictionary<string, float>();
			foreach(string name in Enum.GetNames(typeof(SandoField)))
			{
				currentWeigths[name] = 1;
			}
			currentWeigths[SandoField.Name.ToString()] = 4;
            currentWeigths[SandoField.UnsplitIdentifiers.ToString()] = 9;
			return currentWeigths;
		}
	}
}
