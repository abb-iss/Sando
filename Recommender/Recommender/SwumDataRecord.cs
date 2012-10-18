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
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("SwumNode:{0}{1}{0}", Environment.NewLine, SwumNode.ToString());
            sb.AppendFormat("Action: {0}{1}", ParsedAction, Environment.NewLine);
            sb.AppendFormat("Theme: {0}{1}", ParsedTheme, Environment.NewLine);
            sb.AppendFormat("IndirectObject: {0}{1}", ParsedIndirectObject, Environment.NewLine);
            return sb.ToString();
        }
    }
}
