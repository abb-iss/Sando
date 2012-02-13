using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Sando.Parser.UnitTests
{
	[TestFixture]
	class SplitterTest
	{
		[Test]
		public void TestSplitCamelCase()
		{
			string[] parts = WordSplitter.split("aLongVariableNameInCamelCase");
			Assert.IsTrue(parts.Length == 7);
		}

		[Test]
		public void TestSplitUnderscores()
		{
			string[] parts = WordSplitter.split("a_name_separated_by_lots_of_underscores");
			Assert.IsTrue(parts.Length == 7);
		}

	}
}
