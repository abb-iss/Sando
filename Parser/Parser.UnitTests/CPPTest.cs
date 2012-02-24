using System;
using NUnit.Framework;
using Sando.Core;
using Sando.Parser;

namespace Sando.Parser.UnitTests
{
	[TestFixture]
	public class CPPTest
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
			Generator.Language = LanguageEnum.CPP;
		}

		[Test]
		public void ParseCPPSourceTest()
		{
			bool seenGetTimeMethod = false;
			int numMethods = 0;
			var parser = new SrcMLParser(Generator);
			var elements = parser.Parse("..\\..\\TestFiles\\Event.CPP.txt");
			Assert.IsNotNull(elements);
			Assert.AreEqual(elements.Length, 5);
			foreach(ProgramElement pe in elements)
			{
				if(pe is MethodElement)
				{
					MethodElement method = (MethodElement)pe;
					if(method.Name == "getTime")
					{
						seenGetTimeMethod = true;
						Assert.AreEqual(method.DefinitionLineNumber, 13);
						Assert.AreEqual(method.ReturnType, "double");
						Assert.AreEqual(method.AccessLevel, AccessLevel.Public);
						Assert.AreEqual(method.Arguments, String.Empty);
						Assert.AreEqual(method.Body, "time");
						//Assert.AreNotEqual(method.ClassId, System.Guid.Empty);
					}
					numMethods++;
				}
			}
			Assert.AreEqual(numMethods, 5);
			Assert.IsTrue(seenGetTimeMethod);
		}


		[Test]
		public void ParseCPPHeaderTest()
		{
			bool hasClass = false;
			bool hasEnum = false;
			var parser = new SrcMLParser(Generator);
			var elements = parser.Parse("..\\..\\TestFiles\\Event.H.txt");
			Assert.IsNotNull(elements);
			Assert.AreEqual(elements.Length, 2);
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
					Assert.AreEqual(enumElem.AccessLevel, AccessLevel.Public); //TODO: make sure this is an okay default
					Assert.True(enumElem.FullFilePath.EndsWith("Parser\\Parser.UnitTests\\TestFiles\\Event.H.txt"));
					hasEnum = true;
				}
			}
			Assert.IsTrue(hasClass);
			Assert.IsTrue(hasEnum);
		}

	}
}
