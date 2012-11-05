using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            builder = new UnigramSwumBuilder { Splitter = new CamelIdSplitter() };
            signaturesToSwum = new Dictionary<string, SwumDataRecord>();
            xelementsToSwum = new Dictionary<XElement, SwumDataRecord>();

            //TODO: read SWUM cache file from Sando data directory
            
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

        /// <summary>
        /// The path to the cache file on disk.
        /// </summary>
        public string CachePath { get; private set; }

        /// <summary>
        /// Generates SWUMs for the method definitions within the given source file.
        /// </summary>
        /// <param name="sourcePath">The path to the source file.</param>
        public void AddSourceFile(string sourcePath) {
            var srcmlConverter = new Src2SrcMLRunner(".");
            var tempSrcMLFile = srcmlConverter.GenerateSrcMLFromFile(sourcePath, Path.GetTempFileName());
            try {
                AddSrcMLFile(tempSrcMLFile);
            } finally {
                File.Delete(tempSrcMLFile.FileName);
            }

        }

        /// <summary>
        /// Generates SWUMs for the method definitions within the given SrcML file
        /// </summary>
        /// <param name="srcmlFile">A SrcML file.</param>
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
        /// Initializes the SWUM data from the cache file in the given directory. Any previously constructed SWUMs will be deleted.
        /// </summary>
        /// <param name="cacheDirectory">The path for the directory containing the SWUM cache file.</param>
        public void Initialize(string cacheDirectory) {
            CachePath = Path.Combine(cacheDirectory, "swum-cache.txt");

            if(!File.Exists(CachePath)) {
                Debug.WriteLine("SwumManager.Initialize() - Cache file does not exist: {0}", CachePath);
                return;
            }
            Clear();
            ReadSwumCache(CachePath);
        }

        /// <summary>
        /// Prints the SWUM cache to the file specified in CachePath.
        /// </summary>
        public void PrintSwumCache() {
            PrintSwumCache(CachePath);
        }

        /// <summary>
        /// Prints the SWUM cache to the specified file.
        /// </summary>
        /// <param name="path">The path to print the SWUM cache to.</param>
        public void PrintSwumCache(string path) {
            if(string.IsNullOrWhiteSpace(path)) {
                throw new ArgumentException("Path is empty or null.", "path");
            }
            using(StreamWriter sw = new StreamWriter(path)) {
                PrintSwumCache(sw);
            }
        }

        /// <summary>
        /// Prints the SWUM cache to the specified output stream.
        /// </summary>
        /// <param name="output">A TextWriter to print the SWUM cache to.</param>
        public void PrintSwumCache(TextWriter output) {
            if(output == null) {
                throw new ArgumentNullException("output");
            }
            foreach(var kvp in signaturesToSwum) {
                output.WriteLine("{0}|{1}", kvp.Key, kvp.Value.ToString());
            }
        }

        /// <summary>
        /// Initializes the cache of SWUM data from a file. Any existing SWUM data will be cleared before reading the file.
        /// </summary>
        /// <param name="path">The path to the SWUM cache file.</param>
        public void ReadSwumCache(string path) {
            using(var cacheFile = new StreamReader(path)) {
                //clear any existing SWUMs
                signaturesToSwum.Clear();
                xelementsToSwum.Clear();
                
                //read each SWUM entry from the cache file
                string entry;
                while((entry = cacheFile.ReadLine()) != null) {
                    //the expected format is <signature>|<SwumDataRecord.ToString()>
                    string[] fields = entry.Split(new[] {'|'}, 2);
                    if(fields.Length != 2) {
                        Debug.WriteLine("Too few fields in SWUM cache entry: {0}", entry);
                        continue;
                    }
                    try {
                        string sig = fields[0].Trim();
                        string data = fields[1].Trim();
                        signaturesToSwum[sig] = SwumDataRecord.Parse(data);
                    } catch(FormatException fe) {
                        Debug.WriteLine("Improperly formatted SwumDataRecord in Swum cache entry: {0}", entry);
                    }
                }
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
            //condense consecutive whitespace into a single space
            return Regex.Replace(sig.ToString().Trim(), @"\s+", " ");
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
            //set Action
            if(swumNode.Action != null) {
                record.Action = swumNode.Action.ToPlainString();
                record.ParsedAction = swumNode.Action.GetParse();
            }
            //TODO: action is not lowercased. Should it be?

            //set Theme
            if(swumNode.Theme != null) {
                if(swumNode.Theme is EquivalenceNode && ((EquivalenceNode)swumNode.Theme).EquivalentNodes.Any()) {
                    var firstNode = ((EquivalenceNode)swumNode.Theme).EquivalentNodes[0];
                    record.Theme = firstNode.ToPlainString().ToLower();
                    record.ParsedTheme = firstNode.GetParse();
                } else {
                    record.Theme = swumNode.Theme.ToPlainString().ToLower();
                    record.ParsedTheme = swumNode.Theme.GetParse();
                }
            }

            //set Indirect Object
            if(string.Compare(record.Action, "set", StringComparison.InvariantCultureIgnoreCase) == 0) {
                //special handling for setter methods?
                //TODO: should this set the IO to the declaring class? will that work correctly for sando?
                
            } else {
                if(swumNode.SecondaryArguments != null && swumNode.SecondaryArguments.Any()) {
                    var IONode = swumNode.SecondaryArguments.First();
                    if(IONode.Argument is EquivalenceNode && ((EquivalenceNode)IONode.Argument).EquivalentNodes.Any()) {
                        var firstNode = ((EquivalenceNode)IONode.Argument).EquivalentNodes[0];
                        record.IndirectObject = firstNode.ToPlainString().ToLower();
                        record.ParsedIndirectObject = firstNode.GetParse();
                    } else {
                        record.IndirectObject = IONode.Argument.ToPlainString().ToLower();
                        record.ParsedIndirectObject = IONode.Argument.GetParse();
                    }
                } 
            }

            return record;
        }
        #endregion Protected methods
    }

    
}
