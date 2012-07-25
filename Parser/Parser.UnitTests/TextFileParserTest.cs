using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sando.Parser;

namespace Sando.Parser.UnitTests
{
	[TestFixture]
	class TextFileParserTest
	{

		[Test]
		public void ParseXAMLFile()
		{
			var parser = new TextFileParser();
			var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\SearchViewControl.xaml.txt");
			Assert.IsNotNull(elements);
			Assert.AreEqual(205, elements.Count);
			foreach(var element in elements)
			{
				if(element.DefinitionLineNumber == 218)
				{
					Assert.AreEqual("</UserControl>", element.Name);
				}

			}
		}


		[Test]
		public void ParseXAMLFile2()
		{
			var parser = new XMLFileParser();
			var elements = parser.Parse("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\SearchViewControl.xaml.txt");
			Assert.IsNotNull(elements);
			Assert.AreEqual(84, elements.Count);
			foreach(var element in elements)
			{
				if(element.DefinitionLineNumber == 204)
				{
					Assert.AreEqual("Auto 0 -5,0,0,0 2 0", element.Name);
				}

			}
		}


	}
}
