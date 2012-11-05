using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABB.Swum.Nodes;

namespace Sando.Recommender {
    /// <summary>
    /// Contains various data determined by constructing SWUM on a method.
    /// </summary>
    public class SwumDataRecord {
        public MethodDeclarationNode SwumNode;
        public string Action;
        public PhraseNode ParsedAction;
        public string Theme;
        public PhraseNode ParsedTheme;
        public string IndirectObject;
        public PhraseNode ParsedIndirectObject;

        /// <summary>
        /// Creates a new empty SwumDataRecord.
        /// </summary>
        public SwumDataRecord() {
            SwumNode = null;
            Action = string.Empty;
            ParsedAction = null;
            Theme = string.Empty;
            ParsedTheme = null;
            IndirectObject = string.Empty;
            ParsedIndirectObject = null;
        }

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        public override string ToString() {
            //StringBuilder sb = new StringBuilder();
            //sb.AppendFormat("SwumNode:{0}{1}{0}", Environment.NewLine, SwumNode.ToString());
            //sb.AppendFormat("Action: {0}{1}", ParsedAction, Environment.NewLine);
            //sb.AppendFormat("Theme: {0}{1}", ParsedTheme, Environment.NewLine);
            //sb.AppendFormat("IndirectObject: {0}{1}", ParsedIndirectObject, Environment.NewLine);
            //return sb.ToString();

            return string.Format("{0}|{1}|{2}", ParsedAction, ParsedTheme, ParsedIndirectObject);
        }

        /// <summary>
        /// Creates a SwumDataRecord from a string representation
        /// </summary>
        /// <param name="source">The string to parse the SwumDataRecord from.</param>
        /// <returns>A new SwumDataRecord.</returns>
        public static SwumDataRecord Parse(string source) {
            if(source == null) {
                throw new ArgumentNullException("source");
            }

            string[] fields = source.Split('|');
            if(fields.Length != 3) {
                throw new FormatException(string.Format("Wrong number of |-separated fields. Expected 3, saw {0}", fields.Length));
            }
            var sdr = new SwumDataRecord();
            if(!string.IsNullOrWhiteSpace(fields[0])) {
                sdr.ParsedAction = PhraseNode.Parse(fields[0].Trim());
                sdr.Action = sdr.ParsedAction.ToPlainString();
            }
            if(!string.IsNullOrWhiteSpace(fields[1])) {
                sdr.ParsedTheme = PhraseNode.Parse(fields[1].Trim());
                sdr.Theme = sdr.ParsedTheme.ToPlainString();
            }
            if(!string.IsNullOrWhiteSpace(fields[2])) {
                sdr.ParsedIndirectObject = PhraseNode.Parse(fields[2].Trim());
                sdr.IndirectObject = sdr.ParsedIndirectObject.ToPlainString();
            }
            return sdr;
        }
    }
}
