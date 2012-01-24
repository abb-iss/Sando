using System;
using NUnit.Framework;
using Sando.Core;
using Sando.Parser;

namespace Sando.Parser.UnitTests
{
	[TestFixture]
	public class HelloWorldParserTest
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
			Generator.SetSrcMLLocation(CurrentDirectory + "\\..\\..\\..\\..\\LIBS\\srcML-Win");
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
			String srcML = Generator.GenerateSrcML("..\\..\\TestFiles\\ShortCSharpFile.txt");
			Assert.IsNotNullOrEmpty(srcML);
		}

		[Test]
		public void GenerateSrcMLShortestFileTest()
		{			
			String srcML = Generator.GenerateSrcML("..\\..\\TestFiles\\ShortestCSharpFile.txt");
			Assert.IsNotNullOrEmpty(srcML);
		}

		[Test]
		public void ParseMethodTest()
		{
			bool seenSetLanguageMethod = false;
			var parser = new SrcMLParser();
			var elements = parser.Parse("..\\..\\TestFiles\\ShortCSharpFile.txt");
			Assert.IsNotNull(elements);
			Assert.IsTrue(elements.Length>0);
			foreach(ProgramElement pe in elements)
			{
				if(pe is MethodElement)
				{
					MethodElement method = (MethodElement)pe;
					if(method.Name == "SetLanguage")
					{
						seenSetLanguageMethod = true;
						Assert.AreEqual(method.DefinitionLineNumber, 26);
						Assert.AreEqual(method.ReturnType, "void");
						Assert.AreEqual(method.AccessLevel, AccessLevel.Public);
						//Assert.AreEqual(method.Arguments, "LanguageEnum language");
						//Assert.IsNotNullOrEmpty(method.Body);
					}
				}
			}
			Assert.IsTrue(seenSetLanguageMethod);
		}

		[Test]
		public void BasicParserTest()
		{
			SrcMLParser parser = new SrcMLParser(Generator);
			var elements = parser.Parse("..\\..\\TestFiles\\ShortCSharpFile.txt");
			Assert.IsNotNull(elements);
			Assert.IsTrue(elements.Length>0);
			bool hasClass=false, hasMethod=false;
			foreach (var programElement in elements)
			{
				if(programElement as MethodElement != null)
					hasMethod = true;
				if(programElement as ClassElement != null)
					hasClass = true;
			}
			Assert.IsTrue(hasClass && hasMethod);
		}

		[TearDown]
		public static void CleanUp()
		{
			System.IO.File.Delete(HelloWorldFile);
		}
	
	}
}
