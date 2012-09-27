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

            if(char.IsUpper(query[0])) {
                return new string[] { "starts", "with", "upper", "case" };
            } else {
                return new string[] {"begins", "with", "lower", "case"};
            }
        }
    }
}
