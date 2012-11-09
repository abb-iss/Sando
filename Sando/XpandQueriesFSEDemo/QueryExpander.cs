using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.QueryContracts;

namespace XpandQueriesFSEDemo
{
    public class QueryExpander : IQueryRewriter
    {
        public string RewriteQuery(string query)
        {
            //expand common abbreviations
            if(String.IsNullOrEmpty(query))
            {
                return query;
            }
            else
            {
                return query.Replace("calc", "calculate");
            }
        }
    }
}
