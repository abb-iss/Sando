using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Parser
{
    /// <summary>
    /// This parser handles XML and XAML files. The algorithm to break an XML file to program elements is:
    /// 1. Breaking the document to xml elements.
    /// 2. For each element, calculating the length of the element. If the elment is too long, then folding 
    ///     part of its decendant until the length of the element is under the limit or no decendant can be 
    ///     folded any longer.
    /// 3. For those elements that are under limit by themselves, leaving them alone.
    /// 4. After processing all of the elements, each of them has a length under the limit.
    /// 5. Each of these processed element derives a program element.
    /// </summary>
    public class XAMLFileParser : IParser
    {
        /// <summary>
        /// Breaking down xml element, each element should not exceed this lenght.
        /// </summary>
        public const int LengthLimit = 30;

        
        public List<ProgramElement> OldParse(string fileName, XElement documentRoot)
        {
            var allText = File.ReadAllText(fileName);
            var allXElements = ParseXmlRoot(allText).DescendantNodesAndSelf().
                Where(n => n as XElement!= null);
            var allProgramElements = new List<ProgramElement>();
   
            foreach (XElement element in allXElements)
            {
                var root = ParseXmlRoot(allText);
                var ele = MapNodeToRoot(element, root);
                FoldingElement(ele);

                // All information for creating program element.
                String name = element.Name.LocalName;
                String body = ele.ToString();
                int line = GetLineNumber(element);
                int columnn = GetColumnNumber(element);
                String snippet = GetSnippet(ele);
                
                allProgramElements.Add(new XmlXElement(name, body, line, columnn, fileName, snippet));
            }
            return allProgramElements;
        }



       public List<ProgramElement> Parse(string fileName, XElement root)
       {
           var allXElement = root.DescendantNodesAndSelf().Where(n => n as XElement != null);
           var list = new List<ProgramElement>();
           foreach (XElement original in allXElement)
           {
               var copy = XElement.Parse(original.ToString());
               ControlElement(copy, copy);

               // All information for creating program element.
               String name = original.Name.LocalName;
               String body = copy.ToString();
               int line = GetLineNumber(original);
               int columnn = GetColumnNumber(original);
               String snippet = GetSnippet(copy);

               list.Add(new XmlXElement(name, body, line, columnn, fileName, snippet));
           }
           return list;
       }
 



        private void ControlElement(XElement root, XElement currentElement)
        {
            if (GetLineLength(root) > LengthLimit)
            {
                if (!HasGrandChildren(currentElement))
                {
                    currentElement.ReplaceWith(CreateEntirelyFoldedElement(currentElement));
                    return;
                }
                var children = currentElement.Elements().ToArray();
                for (int i = children.Count() - 1; i >= 0; i--)
                {
                    var child = children.ElementAt(i);
                    ControlElement(root, child);
                    if (GetLineLength(root) <= LengthLimit) return;
                }
                currentElement.ReplaceWith(CreateEntirelyFoldedElement(currentElement));
            }
        }


        private bool HasGrandChildren(XElement element)
        {
            return element.Elements().SelectMany(e => e.Elements()).Any();
        }


        /// <summary>
        /// Map an element to its exact counterpart in another copy of the root.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        private XElement MapNodeToRoot(XElement element, XElement root)
        {
            int layer = GetXElementLayer(element);
            int index = GetIndexAmongLayer(element);
            return GetXElementByLayerAndIndex(root, layer, index);
        }

        /// <summary>
        /// Get the starting line number of an element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static int GetLineNumber(XElement element)
        {
            var lineInfo = element as IXmlLineInfo;
            if (lineInfo != null)
            {
                return lineInfo.LineNumber;
            }
            throw new Exception("Cannot get starting line number.");
        }

        /// <summary>
        /// Get the column number of an XML element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static int GetColumnNumber(XElement element)
        {
            var lineInfo = element as IXmlLineInfo;
            if (lineInfo != null)
            {
                return lineInfo.LinePosition;
            }
            throw new Exception("Cannot get column number.");
        }



        /// <summary>
        /// Given an xml element, checking whether its lenght exceeds @LengthLimit; if it does,
        /// folding part of it to a fake node containing only its local name; Keep doing so until
        /// the element's length is in the limit.
        /// </summary>
        /// <param name="element"></param>
        private void FoldingElement(XElement element)
        {
            var it = new ExpandingCountsIterator(element);
            while (it.HasNextCounts())
            {
                var expandingCounts = it.GetNextCounts();
                // LocalFileWriter.write(it.ConvertCountsToString(expandingCounts));
                FoldingXml(element, e => IsXElementFolding(e, expandingCounts));
                if (GetLineLength(element) < LengthLimit)
                    return;
            }
        }

        private XElement ParseXmlRoot(String source)
        {
            return XDocument.Parse(source, LoadOptions.SetLineInfo | 
                LoadOptions.PreserveWhitespace).Root;
        }
        /// <summary>
        /// Given an xml element, this class keeps track of the decendant that should be folded.
        /// The initial state is that all decendents are expanded; while the final state is no decendant of
        /// the element is expanded.
        /// </summary>
        private class ExpandingCountsIterator
        {
            private readonly XElement element;
            private readonly XElement root;
            private readonly List<int> finalExpandingCounts;
            private List<int> nextExpandingCounts;

            public ExpandingCountsIterator(XElement element)
            {
                this.element = element;
                this.root = GetRoot(element);
                this.finalExpandingCounts = GetFinalExpandingCounts();
                this.nextExpandingCounts = GetInitialExpandingCounts();
            }


            private List<int> GetInitialExpandingCounts()
            {
                var indexes = GetFinalExpandingCounts();
                for (var layer = GetXElementLayer(element) + 1; GetXElementsAtLayer(root, layer).
                    Count > 0; layer++)
                {
                    indexes.Add(GetXElementsAtLayer(root, layer).Count);
                }
                return indexes;
            }

            private List<int> GetFinalExpandingCounts()
            {
                var indexes = GetXElementIndex(element);
                for (int i = 0; i < indexes.Count; i++)
                {
                    indexes[i]++;
                }
                return indexes;
            }

            private List<int> CalculateNextExpandingCounts()
            {
                var newCounts = new List<int>(nextExpandingCounts);
                if (newCounts.Last() > 1)
                {
                    newCounts[newCounts.Count - 1]--;
                }
                else
                {
                    newCounts.RemoveAt(newCounts.Count - 1);
                }
                return newCounts;
            }

            public Boolean HasNextCounts()
            {
                if (nextExpandingCounts.Count == finalExpandingCounts.Count)
                {
                    return nextExpandingCounts.Last() == finalExpandingCounts.Last();
                }
                return true;
            }

            public List<int> GetNextCounts()
            {
                var result = new List<int>(nextExpandingCounts);
                nextExpandingCounts = CalculateNextExpandingCounts();
                return result;
            }

            public String ConvertCountsToString(IEnumerable<int> counts)
            {
                var sb = new StringBuilder();
                foreach (var count in counts)
                {
                    sb.Append(count);
                }
                return sb.ToString();
            }
        }
    
        /// <summary>
        /// Given a xml node and its decendents that should be folded, the method updates
        /// the node accordingly.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="isFolding"></param>
        private void FoldingXml(XElement parent, Predicate<XElement> isFolding)
        {
            var children = GetChildrenXElements(parent);
            children.Reverse();
            foreach (var child in children)
            {
                FoldingXml(child, isFolding);
            }
           
            if (isFolding.Invoke(parent))
                parent.ReplaceWith(CreateEntirelyFoldedElement(parent));
        }

        private XElement CreateEntirelyFoldedElement(XElement element)
        {
            return new XElement(element.Name.LocalName);
        }

        private Boolean IsXElementFolding(XElement element, IEnumerable<int> 
            expandedChildrenCounts)
        {
            var indexes = GetXElementIndex(element);
            for (int i = 0; i < indexes.Count(); i ++)
            {
                int index = indexes.ElementAt(i);
                int bound = expandedChildrenCounts.Count() > i ? 
                    expandedChildrenCounts.ElementAt(i) : int.MinValue;
                if (index >= bound)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Given an XElement, the method gets its indexes in every layer.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static List<int> GetXElementIndex(XElement element)
        {
            var index = new List<int>();
            for (; element != null; element = element.Parent )
            {
                index.Insert(0, GetIndexAmongLayer(element));
            }
            return index.ToList();
        }

        /// <summary>
        /// Given an XElement, this method returns its index among the layer the element
        /// lies in.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static int GetIndexAmongLayer(XElement element)
        {
            var layer = GetXElementLayer(element);
            var allSiblings = GetXElementsAtLayer(GetRoot(element), layer);
            return allSiblings.IndexOf(element);
        }

        /// <summary>
        /// Given a layer number, this method retrives all the XElements on that layer.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        private static List<XElement> GetXElementsAtLayer(XElement root, int layer)
        {
            var nodes = new List<XElement> {root};
            for (int i = 1; i < layer; i ++)
            {
                nodes = nodes.SelectMany(GetChildrenXElements).ToList();
            }
            return nodes;
        }

        private XElement GetXElementByLayerAndIndex(XElement root, int layer, int index)
        {
            var elements = GetXElementsAtLayer(root, layer);
            return elements.ElementAt(index);
        }


        /// <summary>
        /// The model of an xml documents is a tree where the layer is the distance between the
        /// given node to the very root of the document adding one.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static int GetXElementLayer(XElement element)
        {
            int layer = 0;
            for (XElement parent = element; parent != null; parent = parent.Parent)
            {
                layer ++;
            }
            return layer;
        }

        private static List<XElement> GetChildrenXElements(XElement parent)
        {
            return parent.Elements().ToList();
        }

        private static XElement GetRoot(XElement element)
        {
            XElement e;
            for (e = element; e.Parent != null; e = e.Parent);
            return e;
        }

        /// <summary>
        /// Given an xml element without folding, the method returns its snippet.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private String GetSnippet(XElement element)
        {
            return element.ToString();
        }


        /// <summary>
        /// Calculate the line count of a given xml element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private int GetLineLength(XElement element)
        {
            return element.ToString().Split('\n').Length;
        }


        public List<ProgramElement> Parse(string filename)
        {
            var allText = File.ReadAllText(filename);
            var realRoot = ParseXmlRoot(allText);
            return Parse(filename, realRoot);
        }
    }
}
