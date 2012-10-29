using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.QueryContracts;
using Sando.Indexer.Documents;

namespace Sando.UI.InterleavingExperiment
{
    public class NoWeightsFLT : FeatureLocationTechnique, IQueryWeightsSupplier
    {
        public NoWeightsFLT(string n)
            : base(n)
        { 
        }

        public Dictionary<string, float> GetQueryWeightsValues()
        {
            //initialize all weights to 1
            Dictionary<string, float> currentWeigths = new Dictionary<string, float>();
            foreach (string name in Enum.GetNames(typeof(SandoField)))
            {
                currentWeigths[name] = 1;
            }

            return currentWeigths;
        }
    }
}
