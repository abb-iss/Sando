using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using NUnit.Framework;
using Sando.ExtensionContracts.ProgramElementContracts;
using UnitTestHelpers;
using System.Diagnostics;

namespace Sando.Parser.UnitTests
{
	[TestFixture]
	public class CppParserTest
	{
		private static string CurrentDirectory;

		[SetUp]
		public static void Init()
		{
			//set up generator
			CurrentDirectory = Environment.CurrentDirectory;
		}

		[Test]
		public void ParseCPPSourceTest()
		{
			bool seenGetTimeMethod = false;
			int numMethods = 0;
			///////string sourceFile = @"..\..\Parser\Parser.UnitTests\TestFiles\Event.CPP.txt";
            string sourceFile = @"..\..\Parser\Parser.UnitTests\TestFiles\Event.cpp";
            var parser = new SrcMLCppParser();
			var elements = parser.Parse(sourceFile);
			Assert.IsNotNull(elements);
			Assert.AreEqual(elements.Count, 6);
            ///////CheckParseOfEventFile(parser, sourceFile, elements);

		}


		[Test]
		public void ParseCPPHeaderTest()
		{
			bool hasClass = false;
			bool hasEnum = false;
			var parser = new SrcMLCppParser();
			var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\Event.H.txt");
			Assert.IsNotNull(elements);
			Assert.AreEqual(elements.Count, 8);
			foreach(ProgramElement pe in elements)
			{
				if(pe is ClassElement)
				{
					ClassElement classElem = (ClassElement)pe;
					Assert.AreEqual(classElem.Name, "Event");
					Assert.AreEqual(classElem.DefinitionLineNumber, 12);
					Assert.AreEqual(classElem.AccessLevel, AccessLevel.Public);
					Assert.AreEqual(classElem.Namespace, String.Empty);
					Assert.True(classElem.FullFilePath.EndsWith("Parser\\Parser.UnitTests\\TestFiles\\Event.H.txt"));
					hasClass = true;
				}
				else if(pe is EnumElement)
				{
					EnumElement enumElem = (EnumElement)pe;
					Assert.AreEqual(enumElem.Name, "EventType");
					Assert.AreEqual(enumElem.DefinitionLineNumber, 6);
					Assert.AreEqual(enumElem.Namespace, String.Empty);
					Assert.AreEqual(enumElem.Values, "SENSED_DATA_READY SENDING_DONE RECEIVING_DONE");
					Assert.AreEqual(enumElem.AccessLevel, AccessLevel.Public);
					Assert.True(enumElem.FullFilePath.EndsWith("Parser\\Parser.UnitTests\\TestFiles\\Event.H.txt"));
					hasEnum = true;
				}
			}
			Assert.IsTrue(hasClass);
			Assert.IsTrue(hasEnum);
		}

        [Test]
        public void ParseAboutDlgTest()
        {        
            var parser = new SrcMLCppParser();
            var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\AboutDlg.cpp");
            Assert.IsNotNull(elements);
        }

		[Test]
		public void ParseUndefinedNameEnumTest()
		{
			bool hasEnum = false;
			var parser = new SrcMLCppParser();
			var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\PlayMp3Dlg.h.txt");
			Assert.IsNotNull(elements);
			foreach(ProgramElement pe in elements)
			{
				if(pe is EnumElement)
				{
					EnumElement enumElem = (EnumElement)pe;
					Assert.AreEqual(enumElem.Name, "");
					Assert.AreEqual(enumElem.DefinitionLineNumber, 30);
					Assert.AreEqual(enumElem.Values, "IDD IDD_PLAYMP3_DIALOG");
					Assert.AreEqual(enumElem.AccessLevel, AccessLevel.Public);
					Assert.True(enumElem.FullFilePath.EndsWith("Parser\\Parser.UnitTests\\TestFiles\\PlayMp3Dlg.h.txt"));
					hasEnum = true;
				}
			}
			Assert.IsTrue(hasEnum);
		}

		[Test]
		public void ParseAnotherEnumTest()
		{
			var parser = new SrcMLCppParser();
			var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\xmlMatchedTagsHighlighter.cpp");
			Assert.IsNotNull(elements);
		}

		[Test]
		public void TrickyFileTest()
		{
			var parser = new SrcMLCppParser();
			var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\Parameters.h");
			Assert.IsNotNull(elements);
		}

		[Test]
		public void WeirdStructTest()
		{	
			//Note: may not want to create this in mydocuments.... 
			//create a test file
			String WeirdStruct = "struct LangMenuItem { LangType _langType; int	_cmdID; generic_string _langName; " +
									"LangMenuItem(LangType lt, int cmdID = 0, generic_string langName = TEXT(\"\")): " +
									"_langType(lt), _cmdID(cmdID), _langName(langName){};};";
			String WeirdStructFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\HelloWorld1.cs";
			System.IO.File.WriteAllText(WeirdStructFile, WeirdStruct);

            bool hasStruct = false;
            Guid structId = Guid.Empty;
			var parser = new SrcMLCppParser();
			var elements = parser.Parse(WeirdStructFile);
			Assert.IsTrue(elements.Count == 2);

            foreach (ProgramElement pe in elements)
            {
                if (pe is StructElement)
                {
                    StructElement structElement = (StructElement)pe;
                    Assert.IsNotNull(structElement);
                    Assert.AreEqual(structElement.Name, "LangMenuItem");
                    structId = structElement.Id;
                    hasStruct = true;
                }
                else if (pe is MethodElement)
                {
                    MethodElement methodElement = (MethodElement)pe;
                    Assert.AreEqual(methodElement.ClassId, structId);
                }
            }
            Assert.IsTrue(hasStruct);

			//delete file
			System.IO.File.Delete(WeirdStructFile);
		}

		[Test]
		public void ParseCppConstructorTest()
		{
			bool hasConstructor = false;
			var parser = new SrcMLCppParser();
			var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\Event.H.txt");
			Assert.IsNotNull(elements);
			foreach(ProgramElement pe in elements)
			{
				if(pe is MethodPrototypeElement)
				{
					var protoElement = (MethodPrototypeElement)pe;
					if(protoElement.IsConstructor)
					{
						hasConstructor = true;
						Assert.AreEqual(protoElement.Name, "Event");
						Assert.AreEqual(protoElement.DefinitionLineNumber, 15);
					}
				}
			}
			Assert.IsTrue(hasConstructor);
		}

        [Test]
        public void ParseBigFileTest()
        {
            var _processFileInBackground = new System.ComponentModel.BackgroundWorker();
            _processFileInBackground.DoWork +=
                new DoWorkEventHandler(_processFileInBackground_DoWork);	
            _processFileInBackground.RunWorkerAsync();
			Thread.Sleep(5000);
            Assert.IsTrue(_processFileInBackground.IsBusy==false);
        }

		[TestFixtureSetUp]
		public void SetUp()
		{
			TestUtils.InitializeDefaultExtensionPoints();
		}

	    private void _processFileInBackground_DoWork(object sender, DoWorkEventArgs e)
	    {
            var parser = new SrcMLCppParser();
            var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\xmlMatchedTagsHighlighter.cpp");     
	    }

        [Test]
        public void ParseCppSourceWithAlternativeParserTest()
        {
            string sourceFile = @"..\..\Parser\Parser.UnitTests\TestFiles\Event.CPP.txt";
            var parser = new MySrcMlCppParser();
            var elements = parser.Parse(sourceFile);
            Assert.IsNotNull(elements);
            Assert.AreEqual(elements.Count, 6);
            CheckParseOfEventFile(parser, sourceFile, elements);
            bool foundOne = false;
            foreach (var programElement in elements)
            {
                if(programElement as MyMethodElementUnresolvedType !=null)
                {
                    foundOne = true;
                    Assert.IsTrue((programElement as MyMethodElementUnresolvedType).CustomStuffHere.Equals("WHOA"));
                }
            }
            Assert.IsTrue(foundOne);
        }

	    private static void CheckParseOfEventFile(SrcMLCppParser parser, string sourceFile, List<ProgramElement> elements)
	    {
	        bool seenGetTimeMethod = false;
	        int numMethods = 0;
	        foreach (ProgramElement pe in elements)
	        {
	            if (pe is CppUnresolvedMethodElement)
	            {
	                numMethods++;

	                //Resolve
	                bool isResolved = false;
	                MethodElement method = null;
	                CppUnresolvedMethodElement unresolvedMethod = (CppUnresolvedMethodElement) pe;
	                foreach (String headerFile in unresolvedMethod.IncludeFileNames)
	                {
	                    //it's reasonable to assume that the header file path is relative from the cpp file,
	                    //as other included files are unlikely to be part of the same project and therefore 
	                    //should not need to be parsed
	                    string headerPath = System.IO.Path.GetDirectoryName(sourceFile) + "\\" + headerFile;
	                    if (!System.IO.File.Exists(headerPath)) continue;

	                    isResolved = unresolvedMethod.TryResolve(unresolvedMethod, parser.Parse(headerPath), out method);
	                    if (isResolved == true) break;
	                }
	                Assert.IsTrue(isResolved);
	                Assert.IsNotNull(method);

	                //pick one of the resolved methods to see if it seems complete
	                if (method.Name == "getTime")
	                {
	                    seenGetTimeMethod = true;
	                    Assert.AreEqual(method.DefinitionLineNumber, 13);
	                    Assert.AreEqual(method.ReturnType, "double");
	                    Assert.AreEqual(method.AccessLevel, AccessLevel.Public);
	                    Assert.AreEqual(method.Arguments, String.Empty);
	                    Assert.AreEqual(method.Body, "_time");
	                    Assert.AreNotEqual(method.ClassId, System.Guid.Empty);
	                }
	            }
	        }
	        Assert.AreEqual(numMethods, 6);
	        Assert.IsTrue(seenGetTimeMethod);
	    }
	}

    public class MySrcMlCppParser : SrcMLCppParser
    {
        public override MethodElement ParseCppFunction(System.Xml.Linq.XElement function, List<ProgramElement> programElements, string fileName, string[] includedFiles, Type resolvedType, Type unresolvedType, bool isConstructor = false)
        {
            var methodElement = base.ParseCppFunction(function, programElements, fileName, includedFiles, typeof(MyMethodElementType),  typeof(MyMethodElementUnresolvedType), isConstructor);
            var myMethodElement = methodElement as MyMethodElementUnresolvedType;
            myMethodElement.CustomStuffHere = "WHOA";
            return myMethodElement;
        }
    }

    public class MyMethodElementUnresolvedType:CppUnresolvedMethodElement
    {
        [CustomIndexField]
        public string CustomStuffHere { get; set; }

        public MyMethodElementUnresolvedType(string name, int definitionLineNumber, string fullFilePath, string snippet, string arguments, string returnType, string body, string className, bool isConstructor, string[] headerFiles) : base(name, definitionLineNumber, fullFilePath, snippet, arguments, returnType, body, className, isConstructor, headerFiles)
        {
        }

        protected override Type GetResolvedType()
        {
            return typeof(MyMethodElementType);
        }


    }

    public class MyMethodElementType : MethodElement
    {
        [CustomIndexField]
        public string CustomStuffHere { get; set; }

        public MyMethodElementType(string name, int definitionLineNumber, string fullFilePath, string snippet, AccessLevel accessLevel, string arguments, string returnType, string body, Guid classId, string className, string modifiers, bool isConstructor) : base(name, definitionLineNumber, fullFilePath, snippet, accessLevel, arguments, returnType, body, classId, className, modifiers, isConstructor)
        {
        }
    }
}
