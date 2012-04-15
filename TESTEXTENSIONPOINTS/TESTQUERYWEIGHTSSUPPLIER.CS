using System.Collections.Generic;
using Sando.ExtensionContracts.QueryContracts;

namespace Sando.TestExtensionPoints
{
	public class TestQueryWeightsSupplier : IQueryWeightsSupplier
	{
		public Dictionary<string, float> GetQueryWeightsValues()
		{
			Dictionary<string, float> currentWeigths = new Dictionary<string, float>();
			currentWeigths["field1"] = 2;
			currentWeigths["field2"] = 3;
			return currentWeigths;
		}
	}
}
