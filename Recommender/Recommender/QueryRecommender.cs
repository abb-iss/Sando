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

            var recommendations = new SortedSet<string>();

            //search through all the SWUM data
            Dictionary<string,SwumDataRecord> swumData = SwumManager.Instance.GetSwumData();
            foreach(var swumRecord in swumData.Values) {
                bool addAction = false, addTheme = false, addIndirect = false;

                var actionWords = new string[] {};
                var themeWords = new string[] {};
                var indirectWords = new string[] {};
                bool queryInAction = false, queryInTheme = false, queryInIndirect = false;
                if(!string.IsNullOrWhiteSpace(swumRecord.Action)) {
                    actionWords = swumRecord.Action.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                    if(actionWords.Contains(query, StringComparer.InvariantCultureIgnoreCase)) {
                        queryInAction = true;
                    }
                }
                if(!string.IsNullOrWhiteSpace(swumRecord.Theme)) {
                    themeWords = swumRecord.Theme.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if(themeWords.Contains(query, StringComparer.InvariantCultureIgnoreCase)) {
                        queryInTheme = true;
                    }
                }
                if(!string.IsNullOrWhiteSpace(swumRecord.IndirectObject)) {
                    indirectWords = swumRecord.IndirectObject.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if(indirectWords.Contains(query, StringComparer.InvariantCultureIgnoreCase)) {
                        queryInIndirect = true;
                    }
                }

                if(queryInAction||queryInTheme||queryInIndirect) {
                    //add words from action
                    foreach(var word in actionWords) {
                        if(string.Compare(query, word, StringComparison.InvariantCultureIgnoreCase) != 0) {
                            recommendations.Add(string.Format("{0} {1}", query, word));
                        }
                    }
                    if(queryInAction && actionWords.Count() > 2) {
                        recommendations.Add(swumRecord.Action);
                    } else if(!queryInAction && actionWords.Count() > 1) {
                        recommendations.Add(string.Format("{0} {1}", query, swumRecord.Action));
                    }

                    //add words from theme
                    foreach(var word in themeWords) {
                        if(string.Compare(query, word, StringComparison.InvariantCultureIgnoreCase) != 0) {
                            recommendations.Add(string.Format("{0} {1}", query, word));
                        }
                    }
                    if(queryInTheme && themeWords.Count() > 2) {
                        recommendations.Add(swumRecord.Theme);
                    } else if(!queryInTheme && themeWords.Count() > 1) {
                        recommendations.Add(string.Format("{0} {1}", query, swumRecord.Theme));
                    }

                    //add words from indirect object
                    foreach(var word in indirectWords) {
                        if(string.Compare(query, word, StringComparison.InvariantCultureIgnoreCase) != 0) {
                            recommendations.Add(string.Format("{0} {1}", query, word));
                        }
                    }
                    if(queryInIndirect && indirectWords.Count() > 2) {
                        recommendations.Add(swumRecord.IndirectObject);
                    } else if(!queryInIndirect && indirectWords.Count() > 1) {
                        recommendations.Add(string.Format("{0} {1}", query, swumRecord.IndirectObject));
                    }
                }


                
                


                //if(string.Compare(swumRecord.Action, query, StringComparison.InvariantCultureIgnoreCase) == 0) {
                //    addTheme = true;
                //    addIndirect = true;
                //}
                //if(string.Compare(swumRecord.Theme, query, StringComparison.InvariantCultureIgnoreCase) == 0) {
                //    addAction = true;
                //    addIndirect = true;
                //}
                //if(string.Compare(swumRecord.IndirectObject, query, StringComparison.InvariantCultureIgnoreCase) == 0) {
                //    addAction = true;
                //    addTheme = true;
                //}

                //if(addAction) {
                //    string rec = string.Format("{0} {1}", query, swumRecord.Action);
                //    recommendations.Add(rec);
                //}
                //if(addTheme) {
                //    string rec = string.Format("{0} {1}", query, swumRecord.Theme);
                //    recommendations.Add(rec);
                //}
                //if(addIndirect) {
                //    string rec = string.Format("{0} {1}", query, swumRecord.IndirectObject);
                //    recommendations.Add(rec);
                //}
            }

            return recommendations.ToArray();
        }
    }
}
