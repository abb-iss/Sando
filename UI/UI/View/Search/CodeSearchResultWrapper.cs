using Sando.DependencyInjection;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.UI.View.Search.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.UI.View.Search
{
    public class CodeSearchResultWrapper: CodeSearchResult
    {
        private double p;

        public CodeSearchResultWrapper(ExtensionContracts.ProgramElementContracts.ProgramElement programElement, double score):base (programElement,score)
        {
        }

        public string FirstRecommendation
        {
            get
            {
                return ServiceLocator.Resolve<RecommendationGetter>().Convert(this as CodeSearchResult, 1) + " ← " + ServiceLocator.Resolve<RecommendationGetter>().Convert(this as CodeSearchResult, 0) + " ← ";
            }
        }

 


        internal static CodeSearchResult Wrap(CodeSearchResult codeSearchResult)
        {
            return new CodeSearchResultWrapper(codeSearchResult.ProgramElement,codeSearchResult.Score);
        }
    }
}
