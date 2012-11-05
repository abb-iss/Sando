using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ABB.Swum;

namespace Sando.Recommender {
    public class CamelIdSplitter : ConservativeIdSplitter {
        public override string[] Split(string identifier) {
            //do initial conservative split
            var words = base.Split(identifier);
            var result = new List<string>();
            //search for any words that start with two or more uppercase letters, followed by one or more lowercase letters
            foreach(var word in words) {
                var m = Regex.Match(word, @"^(\p{Lu}+)(\p{Lu}\p{Ll}+)$");
                if(m.Success) {
                    //regex matches, split and add each part
                    result.Add(m.Groups[1].Value);
                    result.Add(m.Groups[2].Value);
                } else {
                    result.Add(word);
                }
            }

            return result.ToArray();
        }
    }
}
