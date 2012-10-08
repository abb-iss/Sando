using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using ABB.Swum;
using ABB.Swum.Nodes;
using ABB.SrcML;

namespace Sando.Recommender {
    /// <summary>
    /// Builds SWUM for the methods and method calls in a srcML file.
    /// </summary>
    public class SwumManager {
        private static SwumManager instance;

        private readonly XName[] functionTypes = new XName[] { SRC.Function, SRC.Constructor, SRC.Destructor };
        private SwumBuilder builder;
        private Dictionary<string, SwumDataRecord> signaturesToSwum;
        private Dictionary<XElement, SwumDataRecord> xelementsToSwum;

        
        /// <summary>
        /// Private constructor for a new SwumManager.
        /// </summary>
        private SwumManager() {
            builder = new UnigramSwumBuilder();
            signaturesToSwum = new Dictionary<string, SwumDataRecord>();
            xelementsToSwum = new Dictionary<XElement, SwumDataRecord>();
        }

        /// <summary>
        /// Gets the singleton instance of SwumManager.
        /// </summary>
        public static SwumManager Instance { 
            get {
                if(instance == null) {
                    instance = new SwumManager();
                }
                return instance;
            }
        }

        /// <summary>
        /// Gets or sets the SwumBuilder used to construct SWUM.
        /// </summary>
        public SwumBuilder Builder {
            get { return builder; }
            set { builder = value; }
        }



        public void AddSourceFile(string sourcePath) {
            var srcmlConverter = new Src2SrcMLRunner(".");
            var tempSrcMLFile = srcmlConverter.GenerateSrcMLFromFile(sourcePath, Path.GetTempFileName());
            try {
                AddSrcMLFile(tempSrcMLFile);
            } finally {
                File.Delete(tempSrcMLFile.FileName);
            }

        }

        public void AddSrcMLFile(SrcMLFile srcmlFile) {
            AddSwumForMethodDefinitions(srcmlFile);
        }

        /// <summary>
        /// Clears any constructed SWUMs.
        /// </summary>
        public void Clear() {
            signaturesToSwum.Clear();
            xelementsToSwum.Clear();
        }

        /// <summary>
        /// Constructs SWUM for the methods and method calls in <paramref name="srcFile"/> and caches them internally.
        /// This method will delete any previously constructed SWUMs.
        /// </summary>
        /// <param name="srcFile">The srcML file containing the methods and method calls to build SWUM on.</param>
        public void Initialize(SrcMLFile srcFile) {
            signaturesToSwum.Clear();
            xelementsToSwum.Clear();
            AddSwumForMethodDefinitions(srcFile);
            //AddSwumForMethodCalls(srcFile, srcDb);

        }

        /// <summary>
        /// Prints the SWUM cache to Console.Out.
        /// </summary>
        public void PrintSwumCache() {
            PrintSwumCache(Console.Out);
        }

        /// <summary>
        /// Prints the SWUM cache to the specified output stream.
        /// </summary>
        /// <param name="output">A TextWriter to print the SWUM cache to.</param>
        public void PrintSwumCache(TextWriter output) {
            foreach(var kvp in signaturesToSwum) {
                output.WriteLine("Signature: {0}", kvp.Key);
                output.WriteLine(kvp.Value.ToString());
            }
        }

        /// <summary>
        /// Returns the SWUM data for the given method element. 
        /// </summary>
        /// <param name="methodElement">The XElement of the method to get SWUM data about. This element can be a Function, Constructor, Destructor or Call.</param>
        /// <returns>A SwumDataRecord containing the SWUM data about the given method, or null if no data is found.</returns>
        public SwumDataRecord GetSwumForElement(XElement methodElement) {
            if(methodElement == null) { throw new ArgumentNullException("methodElement"); }
            var methodNames = new XName[] { SRC.Function, SRC.Constructor, SRC.Destructor };
            if(!methodNames.Contains(methodElement.Name) && methodElement.Name != SRC.Call) {
                throw new ArgumentException(string.Format("Not a valid method element: {0}", methodElement.Name), "methodElement");
            }

            return xelementsToSwum.ContainsKey(methodElement) ? xelementsToSwum[methodElement] : null;
        }

        /// <summary>
        /// Returns the SWUM data for the given method signature.
        /// </summary>
        /// <param name="methodSignature">The method signature to get SWUM data about.</param>
        /// <returns>A SwumDataRecord containing the SWUM data about the given method, or null if no data is found.</returns>
        public SwumDataRecord GetSwumForSignature(string methodSignature) {
            if(methodSignature == null) { throw new ArgumentNullException("methodSignature"); }

            if(signaturesToSwum.ContainsKey(methodSignature)) {
                return signaturesToSwum[methodSignature];
            } else {
                return null;
            }
        }

        /// <summary>
        /// Returns a dictionary mapping method signatures to their SWUM data.
        /// </summary>
        public Dictionary<string,SwumDataRecord> GetSwumData() {
            return signaturesToSwum;
        } 

        #region Protected methods
        /// <summary>
        /// Constructs SWUMs for each of the methods defined in <paramref name="srcFile"/> and adds them to the cache.
        /// </summary>
        /// <param name="srcFile">The srcML file containing the method definitions.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="srcFile"/> is null.</exception>
        protected void AddSwumForMethodDefinitions(SrcMLFile srcFile) {
            if(srcFile == null) { throw new ArgumentNullException("srcFile"); }

            //iterate over each method definition in the SrcML file
            foreach(XElement file in srcFile.FileUnits) {
                var functions = from func in file.Descendants()
                                where functionTypes.Contains(func.Name)
                                select func;
                foreach(XElement func in functions) {
                    //construct SWUM on the function (if necessary)
                    MethodDeclarationNode mdn = ConstructSwumFromMethodElement(func);
                    string sig = GetMethodSignature(func);
                    if(signaturesToSwum.ContainsKey(sig)) {
                        Console.WriteLine("Found duplicate method signatures!");
                        Console.WriteLine("First: {0}", signaturesToSwum[sig]);
                        Console.WriteLine("Second: {0}", mdn);
                        xelementsToSwum[func] = signaturesToSwum[sig];
                    } else {
                        var swumData = ProcessSwumNode(mdn);
                        signaturesToSwum[sig] = swumData;
                        xelementsToSwum[func] = swumData;
                    }
                }
            }
        }

        ///// <summary>
        ///// Constructs SWUMs for each of the methods called in <paramref name="srcFile"/> and adds them to the cache.
        ///// </summary>
        ///// <param name="srcFile">The srcML file containing the method calls.</param>
        ///// <param name="srcDb">A SrcMLDataContext database constructed on <paramref name="srcFile"/>.</param>
        ///// <exception cref="System.ArgumentNullException"><paramref name="srcFile"/> is null.</exception>
        ///// <exception cref="System.ArgumentNullException"><paramref name="srcDb"/> is null.</exception>
        //protected void AddSwumForMethodCalls(SrcMLFile srcFile, SrcMLDataContext srcDb) {
        //    if(srcFile == null) { throw new ArgumentNullException("srcFile"); }
        //    if(srcDb == null) { throw new ArgumentNullException("srcDb"); }
            
        //    //iterate over each method call in the SrcML file
        //    foreach(XElement file in srcFile.FileUnits) {
        //        foreach(XElement methodCall in file.Descendants(SRC.Call)) {
        //            MethodDefinition methodDef = srcDb.GetDefinitionForMethodCall(methodCall);
        //            //Add SWUM for the method
        //            if(methodDef != null) {
        //                //found a definition for the method call
        //                //check if already in cache, and construct SWUM if necessary
        //                string signature = GetMethodSignature(methodDef.Xml);
        //                if(!signaturesToSwum.ContainsKey(signature)) {
        //                    //method not yet in cache, constuct SWUM
        //                    var mdn = ConstructSwumFromMethodElement(methodDef.Xml, methodDef.MethodClassName);
        //                    SwumDataRecord swumData = ProcessSwumNode(mdn);
        //                    signaturesToSwum[signature] = swumData;
        //                    xelementsToSwum[methodCall] = swumData;
        //                } else {
        //                    //method already in cache, point to existing SWUM
        //                    xelementsToSwum[methodCall] = signaturesToSwum[signature];
        //                }

        //            } else {
        //                //no definition for method
        //                MethodContext mc = ContextBuilder.BuildMethodContextFromCall(methodCall, srcDb);
        //                string methodName = SrcMLHelper.GetNameForMethod(methodCall).Value;
        //                string sig = GetMethodSignatureFromCall(methodName, mc);
        //                //create SWUM for method, if necessary
        //                if(!signaturesToSwum.ContainsKey(sig)) {
        //                    MethodDeclarationNode mdn = new MethodDeclarationNode(methodName, mc);
        //                    builder.ApplyRules(mdn);
        //                    SwumDataRecord swumData = ProcessSwumNode(mdn);
        //                    signaturesToSwum[sig] = swumData;
        //                    xelementsToSwum[methodCall] = swumData;
        //                } else {
        //                    //method already in cache, point to existing SWUM
        //                    xelementsToSwum[methodCall] = signaturesToSwum[sig];
        //                }
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Constructs SWUM on the given srcML method element. 
        /// </summary>
        /// <param name="methodElement">The srcML element to use. This can be either a Function, Constructor or Destructor.</param>
        /// <returns>A MethodDeclarationNode with SWUM rules applied to it.</returns>
        protected MethodDeclarationNode ConstructSwumFromMethodElement(XElement methodElement) {
            return ConstructSwumFromMethodElement(methodElement, null);
        }

        /// <summary>
        /// Constructs SWUM on the given srcML method element. 
        /// </summary>
        /// <param name="methodElement">The srcML element to use. This can be either a Function, Constructor or Destructor.</param>
        /// <param name="className">The class on which this method is declared.</param>
        /// <returns>A MethodDeclarationNode with SWUM rules applied to it.</returns>
        protected MethodDeclarationNode ConstructSwumFromMethodElement(XElement methodElement, string className) {
            if(!functionTypes.Contains(methodElement.Name)) {
                throw new ArgumentException("Element not a valid method type.", "methodElement");
            }

            string funcName = SrcMLHelper.GetNameForMethod(methodElement).Value;
            MethodContext mc = ContextBuilder.BuildMethodContext(methodElement);
            //set the declaring class name, if it's been passed in
            //this is necessary because the xml from the database for inline methods won't have the surrounding class xml
            if(string.IsNullOrEmpty(mc.DeclaringClass) && !string.IsNullOrEmpty(className)) {
                mc.DeclaringClass = className;
            }

            MethodDeclarationNode mdn = new MethodDeclarationNode(funcName, mc);
            builder.ApplyRules(mdn);
            return mdn;
        }


        /// <summary>
        /// Gets the method signature from the method definition srcML element.
        /// </summary>
        /// <param name="methodElement">The srcML method element to extract the signature from.</param>
        /// <returns>The method signature</returns>
        protected string GetMethodSignature(XElement methodElement) {
            var blockElement = methodElement.Element(SRC.Block);
            StringBuilder sig = new StringBuilder();
            foreach(var n in blockElement.NodesBeforeSelf()) {
                if(n.NodeType == XmlNodeType.Element) {
                    sig.Append(((XElement)n).Value);
                } else if(n.NodeType == XmlNodeType.Text || n.NodeType == XmlNodeType.Whitespace || n.NodeType == XmlNodeType.SignificantWhitespace) {
                    sig.Append(((XText)n).Value);
                }
            }
            return sig.ToString().TrimEnd();
        }

        /// <summary>
        /// Constructs a method signature based on a method call.
        /// </summary>
        /// <param name="name">The name of the method being called.</param>
        /// <param name="mc">A MethodContext object populated with data from the method call.</param>
        /// <returns>A method signature.</returns>
        protected string GetMethodSignatureFromCall(string name, MethodContext mc) {
            if(name == null) { throw new ArgumentNullException("name"); }
            if(name == string.Empty) { throw new ArgumentException("The method name must be non-empty.", "name"); }
            if(mc == null) { throw new ArgumentNullException("mc"); }
            
            StringBuilder sig = new StringBuilder();
            if(mc.IsStatic) {
                sig.Append("static");
            }
            if(!string.IsNullOrEmpty(mc.IdType)) {
                sig.AppendFormat(" {0}", mc.IdType);
            }
            //add method name
            if(!string.IsNullOrEmpty(mc.DeclaringClass)) {
                sig.AppendFormat(" {0}::{1}(", mc.DeclaringClass, name);
            } else {
                sig.AppendFormat(" {0}(", name);
            }
            //add method parameters
            if(mc.FormalParameters.Count > 0) {
                for(int i = 0; i < mc.FormalParameters.Count - 1; i++) {
                    sig.AppendFormat("{0}, ", mc.FormalParameters[i].ParameterType);
                }
                sig.Append(mc.FormalParameters.Last().ParameterType);
            }
            sig.Append(")");
            return sig.ToString().TrimStart(' ');
        }

        /// <summary>
        /// Performs additional processing on a MethodDeclarationNode to put the data in the right format for the Comment Generator.
        /// </summary>
        /// <param name="swumNode">The MethodDeclarationNode from SWUM to process.</param>
        /// <returns>A SwumDataRecord containing <paramref name="swumNode"/> and various data extracted from it.</returns>
        protected SwumDataRecord ProcessSwumNode(MethodDeclarationNode swumNode) {
            var record = new SwumDataRecord();
            record.SwumNode = swumNode;
            if(swumNode.Action != null) {
                record.Action = swumNode.Action.ToPlainString();
            } else {
                record.Action = string.Empty;
            }
            //TODO: action is not lowercased. Should it be?

            //get preposition associated with action (if there is one)
            //record.Preposition = PrepositionManager.GetInstance().GetPreposition(record.Action);

            //Set Theme
            if(swumNode.Theme == null) {
                record.Theme = string.Empty;
            } else if(swumNode.Theme is EquivalenceNode) {
                record.Theme = ((EquivalenceNode)(swumNode.Theme)).EquivalentNodes[0].ToPlainString().ToLower();
            } else {
                record.Theme = swumNode.Theme.ToPlainString().ToLower();
            }

            //clean and process IO
            if(string.Compare(record.Action, "set", StringComparison.InvariantCultureIgnoreCase) == 0) {
                //special handling for setter methods
                //TODO: should this set the IO to the declaring class? will that work correctly for sando?

                //if(record.ThemeLocation == NodeLocation.GivenParameter) {
                //    Console.WriteLine("Bug? Found setter with theme given:");
                //    Console.WriteLine(record.SwumNode);
                //} else {
                //    record.IndirectObject = NodeLocation.Class.ToString();
                //    record.IndirectObjectLocation = NodeLocation.GivenParameter;
                //}
            } else {
                if(swumNode.SecondaryArguments != null && swumNode.SecondaryArguments.Count > 0) {
                    var IONode = swumNode.SecondaryArguments.First();
                    //set IO
                    if(IONode.Argument is EquivalenceNode) {
                        record.IndirectObject = ((EquivalenceNode)(IONode.Argument)).EquivalentNodes[0].ToPlainString().ToLower();
                    } else {
                        record.IndirectObject = IONode.Argument.ToPlainString().ToLower();
                    }
                    ////Set IO location
                    //var swumIOLocation = IONode.Argument.Location;
                    //switch(swumIOLocation) {
                    //    case Location.Name:
                    //        record.IndirectObjectLocation = NodeLocation.PlainMethodName;
                    //        break;
                    //    case Location.Formal:
                    //        record.IndirectObjectLocation = NodeLocation.GivenParameter;
                    //        break;
                    //    default:
                    //        record.IndirectObjectLocation = NodeLocation.None;
                    //        break;
                    //}
                    ////set IO preposition
                    //record.Preposition = IONode.Preposition.ToPlainString();
                } 
                //else if(!string.IsNullOrEmpty(record.Preposition) && record.ThemeLocation != NodeLocation.Class) {
                //    record.IndirectObject = NodeLocation.Class.ToString();
                //    record.IndirectObjectLocation = NodeLocation.Class;
                //}
            }

            return record;
        }
        #endregion Protected methods
    }

    
}
