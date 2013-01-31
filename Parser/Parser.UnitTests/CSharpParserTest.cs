using System;
using System.Collections.Generic;
using NUnit.Framework;
using Sando.ExtensionContracts.ProgramElementContracts;
using UnitTestHelpers;
using System.Linq;
using ABB.SrcML;

namespace Sando.Parser.UnitTests
{
	[TestFixture]
	public class CSharpParserTest
	{
		
		private ABB.SrcML.SrcMLGenerator generator;

        [TestFixtureSetUp]
        public void FixtureSetUp() {
            TestUtils.InitializeDefaultExtensionPoints();
            generator = new ABB.SrcML.SrcMLGenerator(@"LIBS\SrcML");
        }

		[Test]
		public void GenerateSrcMLShortestFileTest() {
		    var parser = new SrcMLCSharpParser(generator);
            var srcML = parser.Parse("TestFiles\\ShortestCSharpFile.txt");
			Assert.IsTrue(srcML!=null);
		}

        [Test]
        public void ParsePossiblyFailingFile()
        {
            var parser = new SrcMLCSharpParser(generator);
            var elements = parser.Parse("TestFiles\\MESTParsingFile.txt");
            Assert.IsNotNull(elements);
            Assert.IsTrue(elements.Count > 0);

        }

	    [Test]
		public void ParseMethodTest()
		{
			var parser = new SrcMLCSharpParser(generator);
			var elements = parser.Parse("TestFiles\\ShortCSharpFile.txt");
			Assert.IsNotNull(elements);
			Assert.IsTrue(elements.Count>0);

            CheckParseOfShortCSharpFile(elements);
		}

        [Test]
        public void ParseMethodWithAlternativeParserTest()
        {            
            var parser = new MySrcMLCSharpParser(generator);
            var elements = parser.Parse("TestFiles\\ShortCSharpFile.txt");
            Assert.IsNotNull(elements);
            Assert.IsTrue(elements.Count > 0);

            CheckParseOfShortCSharpFile(elements);
            bool seenOne = false;
            foreach (var programElement in elements)
            {
                if(programElement as MyCSharpMethodElement !=null)
                {
                    seenOne = true;
                    Assert.IsTrue((programElement as MyCSharpMethodElement).CustomCrazyStuff.Equals("wow"));
                }
            }
            Assert.IsTrue(seenOne);
        }

	    private static void CheckParseOfShortCSharpFile(List<ProgramElement> elements)
	    {
	        bool seenSetLanguageMethod = false;
	        foreach (ProgramElement pe in elements)
	        {
	            if (pe is MethodElement)
	            {
	                MethodElement method = (MethodElement) pe;
	                if (method.Name == "SetLanguage")
	                {
	                    seenSetLanguageMethod = true;
	                    Assert.AreEqual(method.DefinitionLineNumber, 26);
	                    Assert.AreEqual(method.ReturnType, "void");
	                    Assert.AreEqual(method.AccessLevel, AccessLevel.Public);
	                    Assert.AreEqual(method.Arguments, "LanguageEnum language");
	                    Assert.AreEqual(method.Body.Trim(),
                                        "temporary Language   language language  LanguageEnum CSharp Language   LanguageEnum Java");
	                    Assert.AreNotEqual(method.ClassId, System.Guid.Empty);
	                }
                    if (method.Name == "GenerateSrcML")
                    {
                        seenSetLanguageMethod = true;
                        Assert.AreEqual(method.Body.Trim(),
                                        "check whether filename exists  System IO File Exists filename  filename new ParserException  parser input file name does not exist      filename   parser input file name does not exist      filename LaunchSrcML filename  filename");
                        Assert.AreNotEqual(method.ClassId, System.Guid.Empty);
                    }

	            }
	        }
	        Assert.IsTrue(seenSetLanguageMethod);
	    }

	    [Test]
		public void RunIterativeMethodTest()
		{
			for(int i = 0; i < 500; i++)
			{
				ParseMethodTest();
			}

		}

		[Test]
		public void ParseClassTest()
		{
			bool seenClass = false;
			var parser = new SrcMLCSharpParser(generator);
			var elements = parser.Parse("TestFiles\\ShortCSharpFile.txt");
			Assert.IsNotNull(elements);
			Assert.IsTrue(elements.Count > 0);
			foreach(ProgramElement pe in elements)
			{
				if(pe is ClassElement)
				{
					ClassElement classElem = (ClassElement)pe;
					if(classElem.Name == "SrcMLGenerator")
					{
						seenClass = true;
						Assert.AreEqual(classElem.DefinitionLineNumber, 14);
						Assert.AreEqual(classElem.AccessLevel, AccessLevel.Public);
						Assert.AreEqual(classElem.Namespace, "Sando.Parser");
						Assert.True(classElem.FullFilePath.EndsWith("TestFiles\\ShortCSharpFile.txt"));
					}
				}
			}
			Assert.IsTrue(seenClass);
		}

	

		[Test]
		public void BasicParserTest()
		{
			SrcMLCSharpParser parser = new SrcMLCSharpParser(generator);
			var elements = parser.Parse("TestFiles\\ShortCSharpFile.txt");
			Assert.IsNotNull(elements);
			Assert.IsTrue(elements.Count>0);
			bool hasClass=false, hasMethod=false;
			foreach (var programElement in elements)
			{
				if(programElement as MethodElement != null)
					hasMethod = true;
				if(programElement as ClassElement != null)
					hasClass = true;

				Assert.IsTrue(programElement.RawSource != null);
			}
			Assert.IsTrue(hasClass && hasMethod);
		}

		[Test]
		public void EnumParserTest()
		{
			SrcMLCSharpParser parser = new SrcMLCSharpParser(generator);
			var elements = parser.Parse("TestFiles\\ShortCSharpFile.txt");
			bool hasEnum = false;
			foreach(var programElement in elements)
			{
				if(programElement as EnumElement != null)
				{
					EnumElement enumElem = (EnumElement)programElement;
					Assert.AreEqual(enumElem.Name, "LanguageEnum");
					Assert.AreEqual(enumElem.DefinitionLineNumber, 7);
					Assert.AreEqual(enumElem.Namespace, "Sando.Parser");
					Assert.AreEqual(enumElem.Body, "Java C CSharp");
					Assert.AreEqual(enumElem.AccessLevel, AccessLevel.Public);
					Assert.True(enumElem.FullFilePath.EndsWith("TestFiles\\ShortCSharpFile.txt"));
					hasEnum = true;
				}
			}
			Assert.IsTrue(hasEnum);
		}

        [Test]
        public void CSharpStructParserTest()
        {
            SrcMLCSharpParser parser = new SrcMLCSharpParser(generator);
            var elements = parser.Parse("TestFiles\\Struct1.cs.txt");
            bool hasStruct = false;
            foreach (var programElement in elements)
            {
                if (programElement as StructElement != null)
                {
                    StructElement structElem = (StructElement)programElement;
                    Assert.AreEqual(structElem.Name, "SimpleStruct");
                    Assert.AreEqual(structElem.DefinitionLineNumber, 6);
                    Assert.AreEqual(structElem.Namespace, "SimpleNamespace");
                    Assert.AreEqual(structElem.AccessLevel, AccessLevel.Internal);
                    Assert.True(structElem.FullFilePath.EndsWith("TestFiles\\Struct1.cs.txt"));
                    hasStruct = true;
                }
            }
            Assert.IsTrue(hasStruct);
        }

		[Test]
		public void CSharpRegionTest()
		{
			SrcMLCSharpParser parser = new SrcMLCSharpParser(generator);
			var elements = parser.Parse("TestFiles\\RegionTest.txt");
			Assert.IsNotNull(elements);
			Assert.IsTrue(elements.Count == 2);
			bool hasClass = false, hasMethod = false;
			foreach(var programElement in elements)
			{
				if(programElement as MethodElement != null)
					hasMethod = true;
				if(programElement as ClassElement != null)
					hasClass = true;
			}
			Assert.IsTrue(hasClass && hasMethod);
		}

		[Test]
		public void MethodLinksToClassTest()
		{
		    return;

            //NOTE: this test fails because of a bug in srcML
            //please turn this test back on once we receive a fix 
            //from the srcML guys
			SrcMLCSharpParser parser = new SrcMLCSharpParser();
			var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\ImageCaptureCS.txt");
			ClassElement ImageCaptureClassElement = null;
			bool foundMethod = false;

			// first find the class element
			foreach(ProgramElement pe in elements)
			{
				if(pe is ClassElement)
				{
					ClassElement cls = (ClassElement)pe;
					if(cls.Name == "ImageCapture")
					{
						ImageCaptureClassElement = cls;
					}
				}
			}

			// then the method element that should link to it
			foreach(ProgramElement pe in elements) 
			{
				if(pe is MethodElement)
				{
					MethodElement method = (MethodElement)pe;
					if(method.Name == "CaptureByHdc")
					{
						foundMethod = true;
						Assert.AreEqual(method.ClassId, ImageCaptureClassElement.Id);
						Assert.AreEqual(method.ClassName, ImageCaptureClassElement.Name);
					}
				}
			}

			Assert.IsTrue(foundMethod);
		}


		[Test]
		public void CommentsTest()
		{
			
		    string filePath = "TestFiles\\ImageCaptureCS.txt";
            var elements = ParserTestingUtils.ParseCsharpFile(filePath);
		    MethodElement methodElement = null;
            ClassElement classElement = null;
			bool foundMethodComment = false;
            bool foundClassComment = false;
			string methodCommentBody = String.Empty;
			string classCommentBody = String.Empty;

			//find the method and class element
			foreach(ProgramElement pe in elements)
			{
				if(pe is MethodElement)
				{
					MethodElement method = (MethodElement)pe;
					if(method.Name == "InitializeComponent")
					{
						methodElement = method;
					}

				}
                else if(pe is ClassElement)
                {
                    ClassElement klass = (ClassElement)pe;
                    if(klass.Name == "ImageCapture")
                    {
                        classElement = klass;
                    }
                }
			}
			Assert.IsNotNull(methodElement);
			Assert.IsNotNull(classElement);

			//now see if the comment was properly associated to the method element
			foreach(ProgramElement pe in elements)
			{
				if(pe is DocCommentElement)
				{
					DocCommentElement comment = (DocCommentElement)pe;
					if(comment.DocumentedElementId == methodElement.Id)
					{
						methodCommentBody = comment.Body.Replace("\r\n","");
                        if (methodCommentBody.Equals("/// <summary> /// Required method for Designer support - do not modify /// the contents of this method with the code editor. /// </summary>"))
						{
							foundMethodComment = true;
						}
						Assert.True(comment.FullFilePath.EndsWith("TestFiles\\ImageCaptureCS.txt"));
					}
                    else if(comment.DocumentedElementId == classElement.Id)
					{
                        classCommentBody = comment.Body.Replace("\r\n", "");
                        if (classCommentBody.Equals("/// <summary> /// Represents a class for managing the capturing and saving of screenshots. /// </summary>"))
						{
							foundClassComment = true;
						}
						Assert.True(comment.FullFilePath.EndsWith("TestFiles\\ImageCaptureCS.txt"));
					}
				}
			}
			Assert.IsTrue(foundMethodComment);
            Assert.IsTrue(foundClassComment);
		} 

	    [Test]
		public void ParseConstructorTest()
		{
			bool hasConstructor = false;
			var parser = new SrcMLCSharpParser(generator);
			var elements = parser.Parse("TestFiles\\ShortCSharpFile.txt");
			Assert.IsNotNull(elements);
			foreach(ProgramElement pe in elements)
			{
				if(pe is MethodElement)
				{
					var methodElement = (MethodElement)pe;
					if(methodElement.IsConstructor)
					{
						hasConstructor = true;
						Assert.AreEqual(methodElement.Name, "SrcMLGenerator");
						Assert.AreEqual(methodElement.DefinitionLineNumber, 21);
						Assert.AreEqual(methodElement.AccessLevel, AccessLevel.Public);
					}
				}
			}
			Assert.IsTrue(hasConstructor);
		}


        [Test]
        public void GIVEN_file_with_readonly_and_static_elements_WHEN_parse_method_is_called_THEN_valid_access_levels_are_retrieved()
        {
            var parser = new SrcMLCSharpParser(generator);
            var elements = parser.Parse("TestFiles\\ShortCSharpFile.txt");

            var readonlyField = elements.OfType<FieldElement>().FirstOrDefault(f => f.Name == "Language");
            Assert.IsNotNull(readonlyField);
            Assert.AreEqual(readonlyField.AccessLevel, AccessLevel.Internal);

            var staticMethod = elements.OfType<MethodElement>().FirstOrDefault(f => f.Name == "SetSrcMLLocation");
            Assert.IsNotNull(staticMethod);
            Assert.AreEqual(staticMethod.AccessLevel, AccessLevel.Public);
        }

	}

    public class MySrcMLCSharpParser : SrcMLCSharpParser
    {
        public MySrcMLCSharpParser(ABB.SrcML.SrcMLGenerator generator) {
            this.Generator = generator;
        }

        public override MethodElement ParseMethod(System.Xml.Linq.XElement method, List<ProgramElement> programElements, string fileName, Type mytype, bool isConstructor = false)
        {
            var element = base.ParseMethod(method, programElements, fileName,  typeof(MyCSharpMethodElement), isConstructor);
            (element as MyCSharpMethodElement).CustomCrazyStuff = "wow";
            return element;
        }
    }

    public class MyCSharpMethodElement : MethodElement
    {

        [CustomIndexField]
        public string CustomCrazyStuff { get; set; }

        public MyCSharpMethodElement(string name, int definitionLineNumber, string fullFilePath, string snippet, AccessLevel accessLevel, string arguments, string returnType, string body, Guid classId, string className, string modifiers, bool isConstructor) : base(name, definitionLineNumber, fullFilePath, snippet, accessLevel, arguments, returnType, body, classId, className, modifiers, isConstructor)
        {
        }
    }
}
