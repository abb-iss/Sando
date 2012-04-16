using System;
using System.ComponentModel;
using System.Threading;
using NUnit.Framework;
using Sando.ExtensionContracts.ProgramElementContracts;
using UnitTestHelpers;

namespace Sando.Parser.UnitTests
{
	[TestFixture]
	public class CPPTest
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
			string sourceFile = @"..\..\Parser\Parser.UnitTests\TestFiles\Event.CPP.txt";
			var parser = new SrcMLCppParser();
			var elements = parser.Parse(sourceFile);
			Assert.IsNotNull(elements);
			Assert.AreEqual(elements.Count, 6);
			foreach(ProgramElement pe in elements)
			{
				if(pe is CppUnresolvedMethodElement)
				{
					numMethods++;

					//Resolve
					bool isResolved = false;
					MethodElement method = null;
					CppUnresolvedMethodElement unresolvedMethod = (CppUnresolvedMethodElement)pe;
					foreach(String headerFile in unresolvedMethod.IncludeFileNames) {
						//it's reasonable to assume that the header file path is relative from the cpp file,
						//as other included files are unlikely to be part of the same project and therefore 
						//should not need to be parsed
						string headerPath = System.IO.Path.GetDirectoryName(sourceFile) + "\\" + headerFile;
						if(!System.IO.File.Exists(headerPath)) continue;

						isResolved = unresolvedMethod.TryResolve(parser.Parse(headerPath), out method);
						if(isResolved == true) break;
					}
					Assert.IsTrue(isResolved);
					Assert.IsNotNull(method);

					//pick one of the resolved methods to see if it seems complete
					if(method.Name == "getTime")
					{
						seenGetTimeMethod = true;
						Assert.AreEqual(method.DefinitionLineNumber, 13);
						Assert.AreEqual(method.ReturnType, "double");
						Assert.AreEqual(method.AccessLevel, AccessLevel.Public);
						Assert.AreEqual(method.Arguments, String.Empty);
						Assert.AreEqual(method.Body, "time");
						Assert.AreNotEqual(method.ClassId, System.Guid.Empty);
					}
				}
			}
			Assert.AreEqual(numMethods, 6);
			Assert.IsTrue(seenGetTimeMethod);
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
            Assert.IsTrue(true);
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
            var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\LargeCppFile.txt");     
	    }
	}
}
