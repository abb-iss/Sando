using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace Sando.UI.InterleavingExperiment
{
    public class FeatureLocationTechnique
    {
        public FeatureLocationTechnique(string name)
        {
            Name = name;
        }

        public List<CodeSearchResult> Results { set; get; }
        public string Name { private set; get; }
    }
}

