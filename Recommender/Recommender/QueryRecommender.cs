using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ABB.Swum;
using ABB.Swum.Nodes;

namespace Sando.Recommender {
    public class QueryRecommender {
        /// <summary>
        /// Maps query recommendation strings to an accumulated score for that recommendation
        /// </summary>
        private Dictionary<string, int> recommendations; 
        
        
        public QueryRecommender() {
            recommendations = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        }


        public string[] GenerateRecommendations(string query) {
            if(string.IsNullOrEmpty(query)) {
                return null;
            }
            recommendations.Clear();

            //TODO: split query into words, search each one?

            //WeightByFrequency(query);
            //WeightBySameField(query);
            //WeightBySameField_WordsInOrder(query);
            WeightByPartOfSpeech(query);

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

        /// <summary>
        /// Generates query recommendations. Words that appear in the same field (action/theme/indirect object) as the
        /// query word are weighted higher.
        /// The words in the generated recommendations are sorted in the same order as they appear in the method signature.
        /// </summary>
        /// <param name="query">The query string to create recommended completions for.</param>
        private void WeightBySameField_WordsInOrder(string query) {
            const int NormalWeight = 1;
            const int QueryInFieldWeight = 5;

            //search through all the SWUM data
            Dictionary<string, SwumDataRecord> swumData = SwumManager.Instance.GetSwumData();
            foreach(var swumRecord in swumData.Values) {
                var actionWords = new string[] { };
                var themeWords = new string[] { };
                var indirectWords = new string[] { };
                bool queryInAction = false, queryInTheme = false, queryInIndirect = false;
                int queryActionIndex = -1, queryThemeIndex = -1, queryIndirectIndex = -1;
                if(!string.IsNullOrWhiteSpace(swumRecord.Action)) {
                    actionWords = swumRecord.Action.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    queryActionIndex = Array.FindIndex(actionWords, s => string.Compare(s, query, StringComparison.InvariantCultureIgnoreCase) == 0);
                    if(queryActionIndex > -1) { queryInAction = true; }
                }
                if(!string.IsNullOrWhiteSpace(swumRecord.Theme)) {
                    themeWords = swumRecord.Theme.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    queryThemeIndex = Array.FindIndex(themeWords, s => string.Compare(s, query, StringComparison.InvariantCultureIgnoreCase) == 0);
                    if(queryThemeIndex > -1) { queryInTheme = true; }
                }
                if(!string.IsNullOrWhiteSpace(swumRecord.IndirectObject)) {
                    indirectWords = swumRecord.IndirectObject.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    queryIndirectIndex = Array.FindIndex(indirectWords, s => string.Compare(s, query, StringComparison.InvariantCultureIgnoreCase) == 0);
                    if(queryIndirectIndex > -1) { queryInIndirect = true; }
                }

                if(queryInAction || queryInTheme || queryInIndirect) {
                    //add words from action
                    for(int i = 0; i < actionWords.Length; i++) {
                        if(queryInAction) {
                            if(i < queryActionIndex) {
                                AddRecommendation(string.Format("{0} {1}", actionWords[i], query), QueryInFieldWeight);
                            } else if(queryActionIndex < i) {
                                AddRecommendation(string.Format("{0} {1}", query, actionWords[i]), QueryInFieldWeight);
                            }
                        } else {
                            //the action words do not contain the query word
                            AddRecommendation(string.Format("{0} {1}", actionWords[i], query), NormalWeight);
                        }
                    }
                    if(queryInAction && actionWords.Count() > 2) {
                        AddRecommendation(swumRecord.Action, QueryInFieldWeight);
                    } else if(!queryInAction && actionWords.Count() > 1) {
                        AddRecommendation(string.Format("{0} {1}", swumRecord.Action, query), NormalWeight);
                    }

                    //add words from theme
                    for(int i = 0; i < themeWords.Length; i++) {
                        if(queryInTheme) {
                            if(i < queryThemeIndex) {
                                AddRecommendation(string.Format("{0} {1}", themeWords[i], query), QueryInFieldWeight);
                            } else if(queryThemeIndex < i) {
                                AddRecommendation(string.Format("{0} {1}", query, themeWords[i]), QueryInFieldWeight);
                            }
                        } else {
                            //the theme words do not contain the query word
                            if(queryInAction) {
                                AddRecommendation(string.Format("{0} {1}", query, themeWords[i]), NormalWeight);
                            }
                            if(queryInIndirect) {
                                AddRecommendation(string.Format("{0} {1}", themeWords[i], query), NormalWeight);
                            }
                        }
                    }
                    if(queryInTheme && themeWords.Count() > 2) {
                        AddRecommendation(swumRecord.Theme, QueryInFieldWeight);
                    } else if(!queryInTheme && themeWords.Count() > 1) {
                        if(queryInAction) {
                            AddRecommendation(string.Format("{0} {1}", query, swumRecord.Theme), NormalWeight);
                        }
                        if(queryInIndirect) {
                            AddRecommendation(string.Format("{0} {1}", swumRecord.Theme, query), NormalWeight);
                        }
                    }

                    //add words from indirect object
                    for(int i = 0; i < indirectWords.Length; i++) {
                        if(queryInIndirect) {
                            if(i < queryIndirectIndex) {
                                AddRecommendation(string.Format("{0} {1}", indirectWords[i], query), QueryInFieldWeight);
                            } else if(queryIndirectIndex < i) {
                                AddRecommendation(string.Format("{0} {1}", query, indirectWords[i]), QueryInFieldWeight);
                            }
                        } else {
                            //the indirect object words do not contain the query word
                            AddRecommendation(string.Format("{0} {1}", query, indirectWords[i]), NormalWeight);
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

        /// <summary>
        /// Generates query recommendations. Nouns and verbs are weighted higher than other parts of speech.
        /// The words in the generated recommendations are sorted in the same order as they appear in the method signature.
        /// </summary>
        /// <param name="query">The query string to create recommended completions for.</param>
        private void WeightByPartOfSpeech(string query) {
            const int NormalWeight = 1;
            
            //search through all the SWUM data
            Dictionary<string, SwumDataRecord> swumData = SwumManager.Instance.GetSwumData();
            foreach(var swumRecord in swumData.Values) {
                var actionWords = new Collection<WordNode>();
                var themeWords = new Collection<WordNode>();
                var indirectWords = new Collection<WordNode>();
                bool queryInAction = false, queryInTheme = false, queryInIndirect = false;
                int queryActionIndex = -1, queryThemeIndex = -1, queryIndirectIndex = -1;
                if(swumRecord.ParsedAction != null) {
                    actionWords = swumRecord.ParsedAction.GetPhrase();
                    queryActionIndex = FindWordInPhraseNode(swumRecord.ParsedAction, query);
                    if(queryActionIndex > -1) { queryInAction = true; }
                }
                if(swumRecord.ParsedTheme != null) {
                    themeWords = swumRecord.ParsedTheme.GetPhrase();
                    queryThemeIndex = FindWordInPhraseNode(swumRecord.ParsedTheme, query);
                    if(queryThemeIndex > -1) { queryInTheme = true; }
                }
                if(swumRecord.ParsedIndirectObject != null) {
                    indirectWords = swumRecord.ParsedIndirectObject.GetPhrase();
                    queryIndirectIndex = FindWordInPhraseNode(swumRecord.ParsedIndirectObject, query);
                    if(queryIndirectIndex > -1) { queryInIndirect = true; }
                }

                if(queryInAction || queryInTheme || queryInIndirect) {
                    //add words from action
                    for(int i = 0; i < actionWords.Count; i++) {
                        int wordWeight = GetWeightForPartOfSpeech(actionWords[i].Tag);
                        if(queryInAction) {
                            if(i < queryActionIndex) {
                                AddRecommendation(string.Format("{0} {1}", actionWords[i].Text, query), wordWeight);
                            } else if(queryActionIndex < i) {
                                AddRecommendation(string.Format("{0} {1}", query, actionWords[i].Text), wordWeight);
                            }
                        } else {
                            //the action words do not contain the query word
                            AddRecommendation(string.Format("{0} {1}", actionWords[i].Text, query), wordWeight);
                        }
                    }
                    if(queryInAction && actionWords.Count() > 2) {
                        AddRecommendation(swumRecord.Action, NormalWeight);
                    } else if(!queryInAction && actionWords.Count() > 1) {
                        AddRecommendation(string.Format("{0} {1}", swumRecord.Action, query), NormalWeight);
                    }

                    //add words from theme
                    for(int i = 0; i < themeWords.Count; i++) {
                        int wordWeight = GetWeightForPartOfSpeech(themeWords[i].Tag);
                        if(queryInTheme) {
                            if(i < queryThemeIndex) {
                                AddRecommendation(string.Format("{0} {1}", themeWords[i].Text, query), wordWeight);
                            } else if(queryThemeIndex < i) {
                                AddRecommendation(string.Format("{0} {1}", query, themeWords[i].Text), wordWeight);
                            }
                        } else {
                            //the theme words do not contain the query word
                            if(queryInAction) {
                                AddRecommendation(string.Format("{0} {1}", query, themeWords[i].Text), wordWeight);
                            }
                            if(queryInIndirect) {
                                AddRecommendation(string.Format("{0} {1}", themeWords[i].Text, query), wordWeight);
                            }
                        }
                    }
                    if(queryInTheme && themeWords.Count() > 2) {
                        AddRecommendation(swumRecord.Theme, NormalWeight);
                    } else if(!queryInTheme && themeWords.Count() > 1) {
                        if(queryInAction) {
                            AddRecommendation(string.Format("{0} {1}", query, swumRecord.Theme), NormalWeight);
                        }
                        if(queryInIndirect) {
                            AddRecommendation(string.Format("{0} {1}", swumRecord.Theme, query), NormalWeight);
                        }
                    }

                    //add words from indirect object
                    for(int i = 0; i < indirectWords.Count; i++) {
                        int wordWeight = GetWeightForPartOfSpeech(indirectWords[i].Tag);
                        if(queryInIndirect) {
                            if(i < queryIndirectIndex) {
                                AddRecommendation(string.Format("{0} {1}", indirectWords[i].Text, query), wordWeight);
                            } else if(queryIndirectIndex < i) {
                                AddRecommendation(string.Format("{0} {1}", query, indirectWords[i].Text), wordWeight);
                            }
                        } else {
                            //the indirect object words do not contain the query word
                            AddRecommendation(string.Format("{0} {1}", query, indirectWords[i].Text), wordWeight);
                        }
                    }
                    if(queryInIndirect && indirectWords.Count() > 2) {
                        AddRecommendation(swumRecord.IndirectObject, NormalWeight);
                    } else if(!queryInIndirect && indirectWords.Count() > 1) {
                        AddRecommendation(string.Format("{0} {1}", query, swumRecord.IndirectObject), NormalWeight);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the index of <paramref name="word"/> within <paramref name="phraseNode"/>, or -1 if it's not found.
        /// </summary>
        private int FindWordInPhraseNode(PhraseNode phraseNode, string word) {
            if(phraseNode == null) { throw new ArgumentNullException("phraseNode"); }
            if(word == null) { throw new ArgumentNullException("word"); }
            
            int index = -1;
            for(int i = 0; i < phraseNode.Size(); i++) {
                if(string.Compare(phraseNode[i].Text, word, StringComparison.InvariantCultureIgnoreCase) == 0) {
                    index = i;
                    break;
                }
            }
            return index;
        }

        private int GetWeightForPartOfSpeech(PartOfSpeechTag pos) {
            const int NormalWeight = 1;
            const int PrimaryPosWeight = 5;
            const int PreambleWeight = 0;
            var primaryPos = new[] { PartOfSpeechTag.Noun, PartOfSpeechTag.NounPlural, PartOfSpeechTag.PastParticiple, PartOfSpeechTag.Verb, PartOfSpeechTag.Verb3PS, PartOfSpeechTag.VerbIng };

            int result = NormalWeight;
            if(primaryPos.Contains(pos)) {
                result = PrimaryPosWeight;
            } else if(pos == PartOfSpeechTag.Preamble) {
                result = PreambleWeight;
            }
            return result;
        }
    }
}
