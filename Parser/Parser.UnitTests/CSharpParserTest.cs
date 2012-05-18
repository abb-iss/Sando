using System;
using System.Collections.Generic;
using NUnit.Framework;
using Sando.ExtensionContracts.ProgramElementContracts;
using UnitTestHelpers;

namespace Sando.Parser.UnitTests
{
	[TestFixture]
	public class CSharpParserTest
	{
		static String HelloWorldFile;
		private static string CurrentDirectory;
		private static SrcMLGenerator Generator;

		[SetUp]
		public static void Init()
		{
			//Note: may not want to create this in mydocuments.... 
			//create a test file
			String HelloWorld = "public class Hello1 { public static void Main() { System.Console.WriteLine(\"Hello, World!\"); } }";
			HelloWorldFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\HelloWorld1.cs";
			System.IO.File.WriteAllText(HelloWorldFile, HelloWorld);
			//set up generator
			CurrentDirectory = Environment.CurrentDirectory;
			Generator = new SrcMLGenerator();
			Generator.SetSrcMLLocation(CurrentDirectory + "\\..\\..\\LIBS\\srcML-Win");
		}

		[Test]
		public void GenerateSrcMLBasicFileTest()
		{
			String srcML = Generator.GenerateSrcML(HelloWorldFile);
			Assert.IsNotNullOrEmpty(srcML);
		}

		[Test]
		public void GenerateSrcMLShortFileTest()
		{
			String srcML = Generator.GenerateSrcML("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\ShortCSharpFile.txt");
			Assert.IsNotNullOrEmpty(srcML);
		}

        [Test]
        public void GenerateSrcMLLargeFileTest()
        {
            String srcML = Generator.GenerateSrcML("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\VeryLargeCsFile.txt");
            Assert.IsNotNullOrEmpty(srcML);
        }





		[Test]
		public void GenerateSrcMLShortestFileTest()
		{			
			var parser = new SrcMLCSharpParser();
			var srcML = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\ShortestCSharpFile.txt");
			Assert.IsTrue(srcML!=null);
		}

        [Test]
        public void GenerateSrcMLPossiblyFailingFileTest()
        {
            String srcML = Generator.GenerateSrcML("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\MESTParsingFile.txt");
            Assert.IsNotNullOrEmpty(srcML);
        }

        [Test]
        public void ParsePossiblyFailingFile()
        {
            var parser = new SrcMLCSharpParser();
            var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\MESTParsingFile.txt");
            Assert.IsNotNull(elements);
            Assert.IsTrue(elements.Count > 0);

        }

	    [Test]
		public void ParseMethodTest()
		{
			var parser = new SrcMLCSharpParser();
			var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\ShortCSharpFile.txt");
			Assert.IsNotNull(elements);
			Assert.IsTrue(elements.Count>0);

            CheckParseOfShortCSharpFile(elements);
		}

        [Test]
        public void ParseMethodWithAlternativeParserTest()
        {            
            var parser = new MySrcMLCSharpParser();
            var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\ShortCSharpFile.txt");
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
			var parser = new SrcMLCSharpParser();
			var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\ShortCSharpFile.txt");
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
						Assert.AreEqual(classElem.Namespace, "Sando Parser");
						Assert.True(classElem.FullFilePath.EndsWith("Parser\\Parser.UnitTests\\TestFiles\\ShortCSharpFile.txt"));
					}
				}
			}
			Assert.IsTrue(seenClass);
		}

	

		[Test]
		public void BasicParserTest()
		{
			SrcMLCSharpParser parser = new SrcMLCSharpParser();
			var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\ShortCSharpFile.txt");
			Assert.IsNotNull(elements);
			Assert.IsTrue(elements.Count>0);
			bool hasClass=false, hasMethod=false;
			foreach (var programElement in elements)
			{
				if(programElement as MethodElement != null)
					hasMethod = true;
				if(programElement as ClassElement != null)
					hasClass = true;

				Assert.IsTrue(programElement.Snippet != null);
			}
			Assert.IsTrue(hasClass && hasMethod);
		}

		[Test]
		public void EnumParserTest()
		{
			SrcMLCSharpParser parser = new SrcMLCSharpParser();
			var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\ShortCSharpFile.txt");
			bool hasEnum = false;
			foreach(var programElement in elements)
			{
				if(programElement as EnumElement != null)
				{
					EnumElement enumElem = (EnumElement)programElement;
					Assert.AreEqual(enumElem.Name, "LanguageEnum");
					Assert.AreEqual(enumElem.DefinitionLineNumber, 7);
					Assert.AreEqual(enumElem.Namespace, "Sando Parser");
					Assert.AreEqual(enumElem.Values, "Java C CSharp");
					Assert.AreEqual(enumElem.AccessLevel, AccessLevel.Public);
					Assert.True(enumElem.FullFilePath.EndsWith("Parser\\Parser.UnitTests\\TestFiles\\ShortCSharpFile.txt"));
					hasEnum = true;
				}
			}
			Assert.IsTrue(hasEnum);
		}

		[Test]
		public void CSharpRegionTest()
		{
			SrcMLCSharpParser parser = new SrcMLCSharpParser();
			var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\RegionTest.txt");
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
			
		    string filePath = "..\\..\\Parser\\Parser.UnitTests\\TestFiles\\ImageCaptureCS.txt";
            var elements = ParserTestingUtils.ParseCsharpFile(filePath);
		    MethodElement methodElement = null;
			bool foundComment = false;

			//find the method element
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
			}
			Assert.IsNotNull(methodElement);

			//now see if the comment was properly associated to the method element
			foreach(ProgramElement pe in elements)
			{
				if(pe is DocCommentElement)
				{
					DocCommentElement comment = (DocCommentElement)pe;
					if(comment.DocumentedElementId == methodElement.Id)
					{
						foundComment = true;
						Assert.AreEqual(comment.Body, "summary  Required method for Designer support - do not modify  the contents of this method with the code editor.  </summary>");
						Assert.AreEqual(comment.DefinitionLineNumber, methodElement.DefinitionLineNumber);
						Assert.True(comment.FullFilePath.EndsWith("Parser\\Parser.UnitTests\\TestFiles\\ImageCaptureCS.txt"));
					}
				}
			}
			Assert.IsTrue(foundComment);

		}

 

	    [Test]
		public void ParseConstructorTest()
		{
			bool hasConstructor = false;
			var parser = new SrcMLCSharpParser();
			var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\ShortCSharpFile.txt");
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

		[TearDown]
		public static void CleanUp()
		{
			System.IO.File.Delete(HelloWorldFile);
		}

		[TestFixtureSetUp]
		public void SetUp()
		{
			TestUtils.InitializeDefaultExtensionPoints();
		}
	}

    public class MySrcMLCSharpParser : SrcMLCSharpParser
    {

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
