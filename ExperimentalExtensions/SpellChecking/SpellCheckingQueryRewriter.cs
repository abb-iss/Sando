using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHunspell;
using Sando.ExtensionContracts.QueryContracts;

namespace Sando.ExperimentalExtensions.SpellChecking
{
    public class SpellCheckingQueryRewriter : IQueryRewriter
    {
        static NHunspell.SpellFactory factory;
        private static SpellEngine engine;

        private static void Initialize() {
        if (factory == null)
        {
            LanguageConfig enConfig = new LanguageConfig();
            enConfig.LanguageCode = "en";
            enConfig.HunspellAffFile = @"C:\Users\USDASHE1\Documents\VsProjects\Sando-clone\LIBS\Spelling\en_us.aff";
            enConfig.HunspellDictFile = @"C:\Users\USDASHE1\Documents\VsProjects\Sando-clone\LIBS\Spelling\en_us.dic";            
            engine = new SpellEngine();
            engine.AddLanguage(enConfig);
            
        }
    }

        public string RewriteQuery(string query)
        {
            Initialize();
            var queryWords = query.Split(' ');
            foreach (var queryWord in queryWords)
            {
                if(!engine["en"].Spell(queryWord))
                {
                    var suggestions = engine["en"].Suggest(queryWord);
                    if(suggestions.Count>0)
                    {
                        query = query.Replace(queryWord, suggestions.First());                        
                    }
                }
            }
                             
            return query;
        }
    }
}
