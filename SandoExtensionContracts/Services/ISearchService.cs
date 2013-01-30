using Sando.ExtensionContracts.ResultsReordererContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.ExtensionContracts.Services
{
    public interface ISearchService
    {

        List<CodeSearchResult> Search(String searchCriteria);
    }
}
