using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sando.Core;

namespace Sando.Parser.UnitTests
{
	[TestFixture]
	public class InheritanceParserTest
	{
		private static string CurrentDirectory;
		private static SrcMLGenerator Generator;

		[SetUp]
		public static void Init()
		{
			//set up generator
			CurrentDirectory = Environment.CurrentDirectory;
			Generator = new SrcMLGenerator();
			Generator.SetSrcMLLocation(CurrentDirectory + "\\..\\..\\..\\..\\LIBS\\srcML-Win");
		}

		[Test]
		public void ParseProperties()
		{
			var parser = new SrcMLParser();
			var elements = parser.Parse("..\\..\\TestFiles\\ShortInheritance.txt");
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
		public void ClassInheritanceTest()
		{
			bool seenClass = false;
			var parser = new SrcMLParser();
			var elements = parser.Parse("..\\..\\TestFiles\\InheritanceCSharpFile.txt");
			Assert.IsNotNull(elements);
			Assert.IsTrue(elements.Length > 0);
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
						Assert.AreEqual(classElem.FileName, "InheritanceCSharpFile.txt");
						//TODO - make this not dependent upon your path...
						//Assert.AreEqual(classElem.FullFilePath, "C:\\Users\\kosta\\sando\\Parser\\Parser.UnitTests\\TestFiles\\InheritanceCSharpFile.txt");
						Assert.AreEqual(classElem.ExtendedClasses, "SandoException");
					}
				}
			}
			Assert.IsTrue(seenClass);
		}

	}
}
