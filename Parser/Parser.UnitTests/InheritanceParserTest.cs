using System;
using System.Linq;
using NUnit.Framework;
using Sando.ExtensionContracts.ProgramElementContracts;
using UnitTestHelpers;

namespace Sando.Parser.UnitTests
{
	[TestFixture]
	public class InheritanceParserTest
	{
		private static string CurrentDirectory;

		[SetUp]
		public static void Init()
		{
			//set up generator
			CurrentDirectory = Environment.CurrentDirectory;
		}

		[Test]
		public void ParseProperties()
		{
			var parser = new SrcMLCSharpParser();
			var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\ShortInheritance.txt");
			bool seenClass = false;
			int countProperties = 0;
			foreach (var programElement in elements)
			{
				if(programElement.ProgramElementType == ProgramElementType.Class)
					seenClass = true;
				if(programElement.ProgramElementType == ProgramElementType.Property)
					countProperties++;
			}
			Assert.IsTrue(seenClass);
			Assert.IsTrue(countProperties==6);
		}

		[Test]
		public void ParseMultipleParents()
		{
			var parser = new SrcMLCSharpParser();
			var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\MultiParentTest.txt");
			bool seenClass = false;
			foreach(var programElement in elements)
			{
				if(programElement.ProgramElementType == ProgramElementType.Class)
				{
					seenClass = true;
					var classElement = programElement as ClassElement;
					int numParents = 0;
					//TODO - not sure how we will be able to determine which are interfaces and which are classes
					//might have to just put all but the first one in interfaces?
					if(classElement.ImplementedInterfaces != String.Empty)
					{
						numParents += classElement.ImplementedInterfaces.Split(' ').Count();
					}
					if(classElement.ExtendedClasses != String.Empty)
					{
						numParents += classElement.ExtendedClasses.Split(' ').Count();
					}
					Assert.IsTrue(numParents==4);
				}
			}
			Assert.IsTrue(seenClass);			
		}

		[Test]
		public void ClassInheritanceTest()
		{
			bool seenClass = false;
			var parser = new SrcMLCSharpParser();
			var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\InheritanceCSharpFile.txt");
			Assert.IsNotNull(elements);
			Assert.IsTrue(elements.Count > 0);
			foreach(ProgramElement pe in elements)
			{
				if(pe is ClassElement)
				{
					ClassElement classElem = (ClassElement)pe;
					if(classElem.Name == "IndexerException")
					{
						seenClass = true;
						Assert.AreEqual(classElem.DefinitionLineNumber, 8);
						Assert.AreEqual(classElem.AccessLevel, AccessLevel.Public);
						Assert.AreEqual(classElem.Namespace, "Sando Indexer Exceptions");
						//TODO - make this not dependent upon your path...
						//Assert.AreEqual(classElem.FullFilePath, "C:\\Users\\kosta\\sando\\Parser\\Parser.UnitTests\\TestFiles\\InheritanceCSharpFile.txt");
						Assert.AreEqual(classElem.ExtendedClasses, "SandoException");
					}
				}
			}
			Assert.IsTrue(seenClass);
		}

		[TestFixtureSetUp]
		public void SetUp()
		{
			TestUtils.InitializeDefaultExtensionPoints();
		}

	}
}
