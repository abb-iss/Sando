using System;
using NUnit.Framework;
using Sando.Parser;

namespace Sando.Parser.UnitTests
{
	[TestFixture]
	public class HelloWorldParserTest
	{
		static String HelloWorldFile;

		[SetUp]
		public static void Init()
		{
			//create a test file
			String HelloWorld = "public class Hello1 { public static void Main() { System.Console.WriteLine(\"Hello, World!\"); } }";
			HelloWorldFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\HelloWorld1.cs";
			System.IO.File.WriteAllText(HelloWorldFile, HelloWorld);
		}

		[Test]
		public void GenerateSrcMLTest()
		{
			String srcML = SrcMLGenerator.generateSrcML(HelloWorldFile);
			Assert.IsNotNullOrEmpty(srcML);
		}

		[TearDown]
		public static void CleanUp()
		{
			System.IO.File.Delete(HelloWorldFile);
		}
	
	}
}
