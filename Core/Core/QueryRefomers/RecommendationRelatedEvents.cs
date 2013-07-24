using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.Core.QueryRefomers;

namespace Sando.Core.Logging.Events
{
    public partial class LogEvents
    {
        public static void IssueRecommendedQueries(IReformedQuery[] allQueries)
        {
            if (allQueries.Any())
            {
                var sb = new StringBuilder();
                sb.Append("Issue reformed queries.");
                sb.Append("Original query:");
                sb.AppendLine(allQueries.First().OriginalQueryString);
                sb.AppendLine("Recommended queries:");
                sb.AppendLine(allQueries.Select(CreateReformedQueryMessage).Aggregate
                    ((n1, n2) => n1 + n2));
                DataCollectionLogEventHandlers.WriteInfoLogMessage("IReformedQuery",
                    sb.ToString());
            }
        }

        public static void AddSearchTermsToOriginal(IReformedQuery query)
        {
            if (!query.OriginalQueryString.Equals(query.QueryString))
            {
                var sb = new StringBuilder();
                sb.AppendLine("Add search terms automatically.");
                sb.AppendLine("Original query: " + query.OriginalQueryString);
                sb.AppendLine(CreateReformedQueryMessage(query));
                DataCollectionLogEventHandlers.WriteInfoLogMessage("IReformedQuery", sb.ToString());
            }
        }


        private static string CreateReformedQueryMessage(IReformedQuery query)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Reformed Query: " + query.QueryString); 
            sb.AppendLine("Explanation: " + query.ReformExplanation);
            return sb.ToString();
        }

        public static void SelectRecommendedQuery(String query)
        {
            DataCollectionLogEventHandlers.WriteInfoLogMessage("IReformedQuery", 
                "Selected recommendation: " + query);
        }

        public static void TagCloudShowing(string query)
        {
            var sb = new StringBuilder();
            sb.Append("Render a tag cloud.");
            sb.Append(" Query: " + query);
            DataCollectionLogEventHandlers.WriteInfoLogMessage("TagCloud", sb.ToString());
        }

        public static void AddWordFromTagCloud(string query, string header, string word)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Select a tag." + " Query: " + query);
            sb.AppendLine("Tag is related to: " + header + ". Selected tag: " + word);
            DataCollectionLogEventHandlers.WriteInfoLogMessage("TagCloud", sb.ToString());
        }
    }
}
