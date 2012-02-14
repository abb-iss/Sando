using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		[Test]
		public void TestUnsplittable()
		{
			string[] parts = WordSplitter.split("unsplittable");
			Assert.IsTrue(parts.Length == 1);
		}

		[Test]
		public void TestAbbreviations()
		{
			string[] parts = WordSplitter.split("whatAboutALM");
			Assert.IsTrue(parts.Length == 3);
		}

		[Test]
		public void TestAllCaps()
		{
			string[] parts = WordSplitter.split("WHATIFALLINCAPS");
			Assert.IsTrue(parts.Length == 1);
		}

		[Test]
		public void TestAllCapsUnderscore()
		{
			string[] parts = WordSplitter.split("WHAT_IF_ALL_IN_CAPS");
			Assert.IsTrue(parts.Length == 5);
		}

		[Test]
		public void TestBeginUnderscore()
		{
			string[] parts = WordSplitter.split("_beginInUnderscore");
			Assert.IsTrue(parts.Length == 3);
		}


		[Test]
		public void TestPerformance()
		{
			var watch = new Stopwatch();
			watch.Start();
			for(int i = 0; i < 500; i++)
			{
				string[] parts = WordSplitter.split("_beginInUnderscore");
				Assert.IsTrue(parts.Length == 3);
			}
			watch.Stop();
			Assert.IsTrue(watch.ElapsedMilliseconds<500);
		}

	}
}
