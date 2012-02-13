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
			Generator.SetLanguage(LanguageEnum.CPP);
		}

		[Test]
		public void ParseCPPSourceTest()
		{
			var parser = new SrcMLParser(Generator);
			var elements = parser.Parse("..\\..\\TestFiles\\Event.CPP.txt");
			Assert.IsNotNull(elements);
			Assert.IsTrue(elements.Length>0);
			foreach(ProgramElement pe in elements)
			{
				if(pe is MethodElement)
				{
					MethodElement method = (MethodElement)pe;
					/*
					if(method.Name == "SetLanguage")
					{
						seenSetLanguageMethod = true;
						Assert.AreEqual(method.DefinitionLineNumber, 26);
						Assert.AreEqual(method.ReturnType, "void");
						Assert.AreEqual(method.AccessLevel, AccessLevel.Public);
						Assert.AreEqual(method.Arguments, "LanguageEnum language");
						Assert.AreEqual(method.Body, "Language language language LanguageEnum CSharp Language LanguageEnum Java");
						Assert.AreNotEqual(method.ClassId, System.Guid.Empty);
					}*/
				}
			}
		}


		[Test]
		public void ParseCPPHeaderTest()
		{
			var parser = new SrcMLParser(Generator);
			var elements = parser.Parse("..\\..\\TestFiles\\Event.H.txt");
			Assert.IsNotNull(elements);
			Assert.IsTrue(elements.Length > 0);
			foreach(ProgramElement pe in elements)
			{
				if(pe is MethodElement)
				{
					MethodElement method = (MethodElement)pe;
					/*
					if(method.Name == "SetLanguage")
					{
						seenSetLanguageMethod = true;
						Assert.AreEqual(method.DefinitionLineNumber, 26);
						Assert.AreEqual(method.ReturnType, "void");
						Assert.AreEqual(method.AccessLevel, AccessLevel.Public);
						Assert.AreEqual(method.Arguments, "LanguageEnum language");
						Assert.AreEqual(method.Body, "Language language language LanguageEnum CSharp Language LanguageEnum Java");
						Assert.AreNotEqual(method.ClassId, System.Guid.Empty);
					}*/
				}
			}
		}


	}
}
