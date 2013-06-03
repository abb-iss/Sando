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
			Dictionary<string, float> currentWeigths = new Dictionary<string, float>();
			foreach(string name in Enum.GetNames(typeof(SandoField)))
			{
				currentWeigths[name] = 1;
			}
			currentWeigths[SandoField.Name.ToString()] = 3f;
            currentWeigths[SandoField.Arguments.ToString()] = 0.1f;
            currentWeigths[SandoField.ClassName.ToString()] = 1;
            currentWeigths[SandoField.ExtendedClasses.ToString()] = 0.2f;
            currentWeigths[SandoField.ImplementedInterfaces.ToString()] = 0.2f;
            currentWeigths[SandoField.Namespace.ToString()] = 0.05f;
            currentWeigths[SandoField.ReturnType.ToString()] = 0.2f;
            currentWeigths[SandoField.Body.ToString()] = 4f;
			return currentWeigths;
		}
	}
}
