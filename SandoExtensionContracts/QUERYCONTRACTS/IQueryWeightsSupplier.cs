using System.Collections.Generic;

namespace Sando.ExtensionContracts.QueryContracts
{
	public interface IQueryWeightsSupplier
	{
		Dictionary<string, float> GetQueryWeightsValues();
	}
}
