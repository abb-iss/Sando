using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Recommender {
    public class QueryRecommender {
        public QueryRecommender() {
            recommendations = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Maps query recommendation strings to an accumulated score for that recommendation
        /// </summary>
        private Dictionary<string, int> recommendations; 

        public string[] GenerateRecommendations(string query) {
            if(string.IsNullOrEmpty(query)) {
                return null;
            }
            recommendations.Clear();

            //TODO: split query into words, search each one?

            //WeightByFrequency(query);
            WeightBySameField(query);

            //return the recommendations sorted by score in descending order
            return recommendations.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).ToArray();
        }


        private void AddRecommendation(string rec) {
            AddRecommendation(rec, 1);
        }

        private void AddRecommendation(string rec, int score) {
            int count;
            recommendations.TryGetValue(rec, out count);
            recommendations[rec] = count + score;
        }

        /// <summary>
        /// Generates query recommendations and weights them by their frequency in the SWUM data.
        /// </summary>
        /// <param name="query">The query string to create recommended completions for.</param>
        private void WeightByFrequency(string query) {
            //search through all the SWUM data
            Dictionary<string, SwumDataRecord> swumData = SwumManager.Instance.GetSwumData();
            foreach(var swumRecord in swumData.Values) {
                var actionWords = new string[] { };
                var themeWords = new string[] { };
                var indirectWords = new string[] { };
                bool queryInAction = false, queryInTheme = false, queryInIndirect = false;
                if(!string.IsNullOrWhiteSpace(swumRecord.Action)) {
                    actionWords = swumRecord.Action.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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

                if(queryInAction || queryInTheme || queryInIndirect) {
                    //add words from action
                    foreach(var word in actionWords) {
                        if(string.Compare(query, word, StringComparison.InvariantCultureIgnoreCase) != 0) {
                            AddRecommendation(string.Format("{0} {1}", query, word));
                        }
                    }
                    if(queryInAction && actionWords.Count() > 2) {
                        AddRecommendation(swumRecord.Action);
                    } else if(!queryInAction && actionWords.Count() > 1) {
                        AddRecommendation(string.Format("{0} {1}", query, swumRecord.Action));
                    }

                    //add words from theme
                    foreach(var word in themeWords) {
                        if(string.Compare(query, word, StringComparison.InvariantCultureIgnoreCase) != 0) {
                            AddRecommendation(string.Format("{0} {1}", query, word));
                        }
                    }
                    if(queryInTheme && themeWords.Count() > 2) {
                        AddRecommendation(swumRecord.Theme);
                    } else if(!queryInTheme && themeWords.Count() > 1) {
                        AddRecommendation(string.Format("{0} {1}", query, swumRecord.Theme));
                    }

                    //add words from indirect object
                    foreach(var word in indirectWords) {
                        if(string.Compare(query, word, StringComparison.InvariantCultureIgnoreCase) != 0) {
                            AddRecommendation(string.Format("{0} {1}", query, word));
                        }
                    }
                    if(queryInIndirect && indirectWords.Count() > 2) {
                        AddRecommendation(swumRecord.IndirectObject);
                    } else if(!queryInIndirect && indirectWords.Count() > 1) {
                        AddRecommendation(string.Format("{0} {1}", query, swumRecord.IndirectObject));
                    }
                }
            }
        }

        /// <summary>
        /// Generates query recommendations. Words that appear in the same field (action/theme/indirect object) as the
        /// query word are weighted higher.
        /// </summary>
        /// <param name="query">The query string to create recommended completions for.</param>
        private void WeightBySameField(string query) {
            const int NormalWeight = 1;
            const int QueryInFieldWeight = 5;
            
            
            //search through all the SWUM data
            Dictionary<string, SwumDataRecord> swumData = SwumManager.Instance.GetSwumData();
            foreach(var swumRecord in swumData.Values) {
                var actionWords = new string[] { };
                var themeWords = new string[] { };
                var indirectWords = new string[] { };
                bool queryInAction = false, queryInTheme = false, queryInIndirect = false;
                if(!string.IsNullOrWhiteSpace(swumRecord.Action)) {
                    actionWords = swumRecord.Action.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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

                if(queryInAction || queryInTheme || queryInIndirect) {
                    //add words from action
                    foreach(var word in actionWords) {
                        if(queryInAction) {
                            if(string.Compare(query, word, StringComparison.InvariantCultureIgnoreCase) != 0) {
                                //the action words contain the query word, but this isn't it
                                AddRecommendation(string.Format("{0} {1}", query, word), QueryInFieldWeight);
                            }
                        } else {
                            //the action words do not contain the query word
                            AddRecommendation(string.Format("{0} {1}", query, word), NormalWeight);
                        }
                    }
                    if(queryInAction && actionWords.Count() > 2) {
                        AddRecommendation(swumRecord.Action, QueryInFieldWeight);
                    } else if(!queryInAction && actionWords.Count() > 1) {
                        AddRecommendation(string.Format("{0} {1}", query, swumRecord.Action), NormalWeight);
                    }

                    //add words from theme
                    foreach(var word in themeWords) {
                        if(queryInTheme) {
                            if(string.Compare(query, word, StringComparison.InvariantCultureIgnoreCase) != 0) {
                                //the theme words contain the query word, but this isn't it
                                AddRecommendation(string.Format("{0} {1}", query, word), QueryInFieldWeight);
                            }
                        } else {
                            //the theme words do not contain the query word
                            AddRecommendation(string.Format("{0} {1}", query, word), NormalWeight);
                        }
                    }
                    if(queryInTheme && themeWords.Count() > 2) {
                        AddRecommendation(swumRecord.Theme, QueryInFieldWeight);
                    } else if(!queryInTheme && themeWords.Count() > 1) {
                        AddRecommendation(string.Format("{0} {1}", query, swumRecord.Theme), NormalWeight);
                    }

                    //add words from indirect object
                    foreach(var word in indirectWords) {
                        if(queryInIndirect) {
                            if(string.Compare(query, word, StringComparison.InvariantCultureIgnoreCase) != 0) {
                                //the indirect object words contain the query word, but this isn't it
                                AddRecommendation(string.Format("{0} {1}", query, word), QueryInFieldWeight);
                            }
                        } else {
                            //the indirect object words do not contain the query word
                            AddRecommendation(string.Format("{0} {1}", query, word), NormalWeight);
                        }
                    }
                    if(queryInIndirect && indirectWords.Count() > 2) {
                        AddRecommendation(swumRecord.IndirectObject, QueryInFieldWeight);
                    } else if(!queryInIndirect && indirectWords.Count() > 1) {
                        AddRecommendation(string.Format("{0} {1}", query, swumRecord.IndirectObject), NormalWeight);
                    }
                }
            }
        }

        private void WeightByPartOfSpeech(string query) {
            
        }
    }
}
