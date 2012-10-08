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
        public string Theme;
        //public NodeLocation ThemeLocation;
        public string IndirectObject;
        //public NodeLocation IndirectObjectLocation;
        //public string Preposition;

        /// <summary>
        /// Creates a new empty SwumDataRecord.
        /// </summary>
        public SwumDataRecord() {
            SwumNode = null;
            Action = string.Empty;
            Theme = string.Empty;
            //ThemeLocation = NodeLocation.None;
            IndirectObject = string.Empty;
            //IndirectObjectLocation = NodeLocation.None;
            //Preposition = string.Empty;
        }

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("SwumNode:{0}{1}{0}", Environment.NewLine, SwumNode.ToString());
            sb.AppendFormat("Action: {0}{1}", Action, Environment.NewLine);
            sb.AppendFormat("Theme: {0}{1}", Theme, Environment.NewLine);
            //sb.AppendFormat("ThemeLocation: {0}{1}", ThemeLocation, Environment.NewLine);
            sb.AppendFormat("IndirectObject: {0}{1}", IndirectObject, Environment.NewLine);
            //sb.AppendFormat("IndirectObjectLocation: {0}{1}", IndirectObjectLocation, Environment.NewLine);
            //sb.AppendFormat("Preposition: {0}{1}", Preposition, Environment.NewLine);
            return sb.ToString();
        }
    }
}
