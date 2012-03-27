using System.Diagnostics;
using NUnit.Framework;
using Sando.Core;

namespace Sando.Parser.UnitTests
{
	[TestFixture]
	class SplitterTest
	{
		[Test]
		public void TestSplitCamelCase()
		{
			string[] parts = WordSplitter.Split("aLongVariableNameInCamelCase");
			Assert.IsTrue(parts.Length == 7);
		}

		[Test]
		public void TestSplitUnderscores()
		{
			string[] parts = WordSplitter.Split("a_name_separated_by_lots_of_underscores");
			Assert.IsTrue(parts.Length == 7);
		}

		[Test]
		public void TestUnsplittable()
		{
			string[] parts = WordSplitter.Split("unsplittable");
			Assert.IsTrue(parts.Length == 1);
		}

		[Test]
		public void TestAbbreviations()
		{
			string[] parts = WordSplitter.Split("whatAboutALM");
			Assert.IsTrue(parts.Length == 3);
		}

		[Test]
		public void TestAllCaps()
		{
			string[] parts = WordSplitter.Split("WHATIFALLINCAPS");
			Assert.IsTrue(parts.Length == 1);
		}

		[Test]
		public void TestAllCapsUnderscore()
		{
			string[] parts = WordSplitter.Split("WHAT_IF_ALL_IN_CAPS");
			Assert.IsTrue(parts.Length == 5);
		}

		[Test]
		public void TestBeginUnderscore()
		{
			string[] parts = WordSplitter.Split("_beginInUnderscore");
			Assert.IsTrue(parts.Length == 3);
		}


		[Test]
		public void TestPerformance()
		{
			var watch = new Stopwatch();
			watch.Start();
			for(int i = 0; i < 500; i++)
			{
				string[] parts = WordSplitter.Split("_beginInUnderscore");
				Assert.IsTrue(parts.Length == 3);
			}
			watch.Stop();
			Assert.IsTrue(watch.ElapsedMilliseconds<500);
		}

	}
}
