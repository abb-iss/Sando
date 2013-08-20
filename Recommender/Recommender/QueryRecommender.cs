using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ABB.Swum;
using ABB.Swum.Nodes;
using System.Diagnostics;
using Sando.Core.Tools;
using Sando.DependencyInjection;

namespace Sando.Recommender {
    public class QueryRecommender {
        /// <summary>
        /// Maps query recommendation strings to an accumulated score for that recommendation
        /// </summary>
        
        
        
        public QueryRecommender() {
            
        }


        public ISwumRecommendedQuery[] GenerateRecommendations(string query) {
            if(string.IsNullOrEmpty(query)) {
                if (query != null)
                {
                    return GetAllSearchHistoryItems();
                }
                return new ISwumRecommendedQuery[0];
            }
            try
            {
                return new ISwumRecommendedQuery[0];
                var recommendations = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);


                //TODO: split query into words, search each one? Already implemented for plain method names            

                //WeightByFrequency(query);
                //WeightBySameField(query);
                //WeightBySameField_WordsInOrder(query);
                AddRecommendationForEachTerm(query, recommendations);

                //return the recommendations sorted by score in descending order
                List<KeyValuePair<string, int>> listForSorting = recommendations.ToList();
                listForSorting.Sort((firstPair, nextPair) =>
                    {
                        return nextPair.Value.CompareTo(firstPair.Value);
                    }
                );
                return SortRecommendations(query, listForSorting.Select(kvp => kvp.Key).ToArray());
            }
            catch (Exception e)
            {
                return new ISwumRecommendedQuery[0];
            }
        }

        private ISwumRecommendedQuery[] SortRecommendations(string query, string[] queries)
        {
            return new SwumQueriesSorter().SelectSortSwumRecommendations(query, queries);
        }


        private ISwumRecommendedQuery[] GetAllSearchHistoryItems()
        {
            return new SwumQueriesSorter().GetAllHistoryItems();
        }


        private void AddRecommendation(string rec, Dictionary<string, int> recommendations)
        {
            AddRecommendation(rec, 1, recommendations);
        }

        private void AddRecommendation(string rec, int score, Dictionary<string, int> recommendations)
        {
            int count;
            recommendations.TryGetValue(rec, out count);
            recommendations[rec] = count + score;
        }

        private void AddRecommendation(Dictionary<string, int> recommendations, string p, int NormalWeight, MethodDeclarationNode methodDeclarationNode = null)
        {
            AddRecommendation(p.Trim(), NormalWeight, recommendations);
            if(methodDeclarationNode !=null)
                AddRecommendation(methodDeclarationNode.Name.Trim(), NormalWeight, recommendations);
        }


        private void AddRecommendationForEachTerm(String query, Dictionary<String, int> recommendations)
        {
            var terms = query.Split().Where(t => !String.IsNullOrWhiteSpace(t));
            foreach (var term in terms)
            {
                WeightByPartOfSpeech(term, recommendations);
            }
        }

        /// <summary>
        /// Generates query recommendations. Nouns and verbs are weighted higher than other parts of speech.
        /// The words in the generated recommendations are sorted in the same order as they appear in the method signature.
        /// </summary>
        /// <param name="query">The query string to create recommended completions for.</param>
        private void WeightByPartOfSpeech(string query, Dictionary<string, int> recommendations ) {
            const int NormalWeight = 1;

            var terms = query.Split(' ');

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
                                AddRecommendation(recommendations, string.Format("{0} {1}", actionWords[i].Text, query), wordWeight);
                            } else if(queryActionIndex < i) {
                                AddRecommendation( recommendations, string.Format("{0} {1}", query, actionWords[i].Text), wordWeight);
                            }
                        } else {
                            //the action words do not contain the query word
                            AddRecommendation( recommendations, string.Format("{0} {1}", actionWords[i].Text, query), wordWeight);
                        }
                    }
                    if(queryInAction && actionWords.Count() > 2) {
                        AddRecommendation( recommendations, swumRecord.Action, NormalWeight);
                    } else if(!queryInAction && actionWords.Count() > 1) {
                        AddRecommendation( recommendations, string.Format("{0} {1}", swumRecord.Action, query), NormalWeight);
                    }

                    //add words from theme
                    for(int i = 0; i < themeWords.Count; i++) {
                        int wordWeight = GetWeightForPartOfSpeech(themeWords[i].Tag);
                        if(queryInTheme) {
                            if(i < queryThemeIndex) {
                                AddRecommendation( recommendations, string.Format("{0} {1}", themeWords[i].Text, query), wordWeight);
                            } else if(queryThemeIndex < i) {
                                AddRecommendation( recommendations, string.Format("{0} {1}", query, themeWords[i].Text), wordWeight);
                            }
                        } else {
                            //the theme words do not contain the query word
                            if(queryInAction) {
                                AddRecommendation( recommendations, string.Format("{0} {1}", query, themeWords[i].Text), wordWeight);
                            }
                            if(queryInIndirect) {
                                AddRecommendation( recommendations, string.Format("{0} {1}", themeWords[i].Text, query), wordWeight);
                            }
                        }
                    }
                    if(queryInTheme && themeWords.Count() > 2) {
                        AddRecommendation( recommendations, swumRecord.Theme, NormalWeight);
                    } else if(!queryInTheme && themeWords.Count() > 1) {
                        if(queryInAction) {
                            AddRecommendation( recommendations, string.Format("{0} {1}", query, swumRecord.Theme), NormalWeight);
                        }
                        if(queryInIndirect) {
                            AddRecommendation( recommendations, string.Format("{0} {1}", swumRecord.Theme, query), NormalWeight);
                        }
                    }

                    //add words from indirect object
                    for(int i = 0; i < indirectWords.Count; i++) {
                        int wordWeight = GetWeightForPartOfSpeech(indirectWords[i].Tag);
                        if(queryInIndirect) {
                            if(i < queryIndirectIndex) {
                                AddRecommendation( recommendations, string.Format("{0} {1}", indirectWords[i].Text, query), wordWeight);
                            } else if(queryIndirectIndex < i) {
                                AddRecommendation( recommendations, string.Format("{0} {1}", query, indirectWords[i].Text), wordWeight);
                            }
                        } else {
                            //the indirect object words do not contain the query word
                            AddRecommendation( recommendations, string.Format("{0} {1}", query, indirectWords[i].Text), wordWeight);
                        }
                    }
                    if(queryInIndirect && indirectWords.Count() > 2) {
                        AddRecommendation( recommendations, swumRecord.IndirectObject, NormalWeight);
                    } else if(!queryInIndirect && indirectWords.Count() > 1) {
                        AddRecommendation( recommendations, string.Format("{0} {1}", query, swumRecord.IndirectObject), NormalWeight);
                    }
                }
                AddFullMethodName(query, recommendations, NormalWeight, terms, swumRecord);
            }
        }

        private void AddFullMethodName(string query, Dictionary<string, int> recommendations, int NormalWeight, string[] terms, SwumDataRecord swumRecord)
        {
            if (swumRecord.SwumNode.Name.ToLower().Contains(query.ToLower()))
            {
                AddRecommendation(swumRecord.SwumNode.Name, NormalWeight + (int)(NormalWeight * 10 / Distance(swumRecord.SwumNode.Name, query)), recommendations);
                //Debug.WriteLine(swumRecord.SwumNode.Name + " " + (NormalWeight + (int)(NormalWeight * 10 / Distance(swumRecord.SwumNode.Name, query))));
            }
            else
            {
                bool shouldAdd = true;
                foreach (var term in terms)
                    if (!swumRecord.SwumNode.Name.ToLower().Contains(term))
                        shouldAdd = false;
                if (shouldAdd)
                {
                    AddRecommendation(swumRecord.SwumNode.Name, NormalWeight + (int)(NormalWeight * 10 / Distance(swumRecord.SwumNode.Name, query)), recommendations);
                    //Debug.WriteLine(swumRecord.SwumNode.Name+" "+(NormalWeight + (int)(NormalWeight * 10 / Distance(swumRecord.SwumNode.Name, query))));
                }
            }
        }

        static int maxOffset = 5;

        public static float Distance(string s1, string s2)
        {
            if (String.IsNullOrEmpty(s1))
                return
                String.IsNullOrEmpty(s2) ? 0 : s2.Length;
            if (String.IsNullOrEmpty(s2))
                return s1.Length;
            int c = 0;
            int offset1 = 0;
            int offset2 = 0;
            int lcs = 0;
            while ((c + offset1 < s1.Length)
            && (c + offset2 < s2.Length))
            {
                if (s1[c + offset1] == s2[c + offset2]) lcs++;
                else
                {
                    offset1 = 0;
                    offset2 = 0;
                    for (int i = 0; i < maxOffset; i++)
                    {
                        if ((c + i < s1.Length)
                        && (s1[c + i] == s2[c]))
                        {
                            offset1 = i;
                            break;
                        }
                        if ((c + i < s2.Length)
                        && (s1[c] == s2[c + i]))
                        {
                            offset2 = i;
                            break;
                        }
                    }
                }
                c++;
            }
            var returnVal = (s1.Length + s2.Length) / 2 - lcs;
            return returnVal;
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
