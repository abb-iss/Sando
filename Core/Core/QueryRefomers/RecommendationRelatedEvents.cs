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
                sb.Append("Created links.");
                sb.Append(allQueries.Select(CreateReformedQueryMessage).Aggregate
                    ((n1, n2) => n1 + n2));
                DataCollectionLogEventHandlers.WriteInfoLogMessage("Post-search recommendation",
                    sb.ToString());
            }
        }

        public static void AddSearchTermsToOriginal(IReformedQuery query)
        {
            if (!query.OriginalQueryString.Equals(query.QueryString))
            {
                var sb = new StringBuilder();
                sb.Append("Add search terms automatically.");
                sb.Append(CreateReformedQueryMessage(query));
                DataCollectionLogEventHandlers.WriteInfoLogMessage("Post-search recommendation", sb.ToString());
            }
        }


        private static string CreateReformedQueryMessage(IReformedQuery query)
        {
            var sb = new StringBuilder();
            sb.Append(query.OriginalQueryString + "=>" + query.QueryString + "=>" + 
                query.ReformExplanation + ";");
            return sb.ToString();
        }

        public static void SelectRecommendedQuery(String query, int index)
        {
            DataCollectionLogEventHandlers.WriteInfoLogMessage("Post-search recommendation", 
                "Clicked link: " + index);
        }

        public static void TagCloudShowing(string query)
        {
            var sb = new StringBuilder();
            sb.Append("Render a tag cloud.");
            DataCollectionLogEventHandlers.WriteInfoLogMessage("TagCloud", sb.ToString());
        }

        public static void AddWordFromTagCloud(string query, string header, string word)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Select a tag.");
            DataCollectionLogEventHandlers.WriteInfoLogMessage("TagCloud", sb.ToString());
        }

        public static void SelectHistoryItem()
        {
            DataCollectionLogEventHandlers.WriteInfoLogMessage("Pre-search recommendation", "History");
        }

        public static void ClearHistory()
        {
            DataCollectionLogEventHandlers.WriteInfoLogMessage("Pre-search recommendation", "Clear history");
        }


        public static void SelectSwumRecommendation(string query)
        {
            if (query.Trim().Split().Count() == 1)
            {
                DataCollectionLogEventHandlers.WriteInfoLogMessage("Pre-search recommendation", "Identifier: " + query);
            }
            else
            {
                DataCollectionLogEventHandlers.WriteInfoLogMessage("Pre-search recommendation", "Verb-DO pair: " + query);
            }
        }
    }
}
