using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Recommender {
    public class QueryRecommender {
        public QueryRecommender() {}

        public string[] GenerateRecommendations(string query) {
            if(string.IsNullOrEmpty(query)) {
                return null;
            }
            //TODO: split query into words, search each one

            var recommendations = new HashSet<string>();

            Dictionary<string,SwumDataRecord> swumData = SwumManager.Instance.GetSwumData();
            bool addAction = false, addTheme = false, addIndirect = false;
            foreach(var swumRecord in swumData.Values) {
                if(string.Compare(swumRecord.Action, query, StringComparison.InvariantCultureIgnoreCase) == 0) {
                    addTheme = true;
                    addIndirect = true;
                }
                if(string.Compare(swumRecord.Theme, query, StringComparison.InvariantCultureIgnoreCase) == 0) {
                    addAction = true;
                    addIndirect = true;
                }
                if(string.Compare(swumRecord.IndirectObject, query, StringComparison.InvariantCultureIgnoreCase) == 0) {
                    addAction = true;
                    addTheme = true;
                }

                if(addAction) {
                    recommendations.Add(string.Format("{0} {1}", query, swumRecord.Action));
                }
                if(addTheme) {
                    recommendations.Add(string.Format("{0} {1}", query, swumRecord.Theme));
                }
                if(addIndirect) {
                    recommendations.Add(string.Format("{0} {1}", query, swumRecord.IndirectObject));
                }
            }

            return recommendations.ToArray();

            //if(char.IsUpper(query[0])) {
            //    return new string[] { "starts", "with", "upper", "case" };
            //} else {
            //    return new string[] {"begins", "with", "lower", "case"};
            //}
        }
    }
}
