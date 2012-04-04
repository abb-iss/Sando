using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using NUnit.Framework;
using Sando.Core.Tools;

namespace Sando.Core.UnitTests
{
	[TestFixture]
	class SplitterTest
	{
		[Test]
		public void TestSplitCamelCase()
		{
			string[] parts = WordSplitter.ExtractWords("aLongVariableNameInCamelCase");
			Assert.IsTrue(parts.Length == 7);
		}

		[Test]
		public void TestSplitUnderscores()
		{
			string[] parts = WordSplitter.ExtractWords("a_name_separated_by_lots_of_underscores");
			Assert.IsTrue(parts.Length == 7);
		}

		[Test]
		public void TestUnsplittable()
		{
			string[] parts = WordSplitter.ExtractWords("unsplittable");
			Assert.IsTrue(parts.Length == 1);
		}

		[Test]
		public void TestAbbreviations()
		{
			string[] parts = WordSplitter.ExtractWords("whatAboutALM");
			Assert.IsTrue(parts.Length == 3);
		}

		[Test]
		public void TestAllCaps()
		{
			string[] parts = WordSplitter.ExtractWords("WHATIFALLINCAPS");
			Assert.IsTrue(parts.Length == 1);
		}

		[Test]
		public void TestAllCapsUnderscore()
		{
			string[] parts = WordSplitter.ExtractWords("WHAT_IF_ALL_IN_CAPS");
			Assert.IsTrue(parts.Length == 5);
		}

		[Test]
		public void TestBeginUnderscore()
		{
			string[] parts = WordSplitter.ExtractWords("_beginInUnderscore");
			Assert.IsTrue(parts.Length == 3);
		}

		[Test]
		public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenQuotesUsed()
		{
			List<string> parts = WordSplitter.ExtractSearchTerms("word \"words inside quotes\" another_word");
			Assert.IsTrue(parts.Count == 3);
			Assert.IsTrue(String.Join("*", parts) == "words inside quotes*word*another_word");
		}

		[Test]
		public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenQuotesUsedWithInvalidQuote()
		{
			List<string> parts = WordSplitter.ExtractSearchTerms("word \"words inside quotes\" another\"word");
			Assert.IsTrue(parts.Count == 4);
			Assert.IsTrue(String.Join("*", parts) == "words inside quotes*word*another*word");
		}

		[Test]
		public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenNoQuotesUsed()
		{
			List<string> parts = WordSplitter.ExtractSearchTerms("word words inside quotes another_word");
			Assert.IsTrue(parts.Count == 5);
			Assert.IsTrue(String.Join("*", parts) == "word*words*inside*quotes*another_word");
		}

		[Test]
		public void ExtractSearchTerms_ReturnsEmptyListWhenSearchTermIsEmptyString()
		{
			List<string> parts = WordSplitter.ExtractSearchTerms(String.Empty);
			Assert.IsTrue(parts.Count == 0);
		}

		[Test]
		public void ExtractSearchTerms_ContractFailsWhenSearchTermIsNull()
		{
			try
			{
				WordSplitter.ExtractSearchTerms(null);
			}
			catch
			{
				//contract exception catched here
			}
			Assert.True(contractFailed, "Contract should fail!");
		}


		[Test]
		public void TestPerformance()
		{
			var watch = new Stopwatch();
			watch.Start();
			for(int i = 0; i < 500; i++)
			{
				string[] parts = WordSplitter.ExtractWords("_beginInUnderscore");
				Assert.IsTrue(parts.Length == 3);
			}
			watch.Stop();
			Assert.IsTrue(watch.ElapsedMilliseconds<500);
		}

		[SetUp]
		public void ResetContract()
		{
			contractFailed = false;
			Contract.ContractFailed += (sender, e) =>
			{
				e.SetHandled();
				e.SetUnwind();
				contractFailed = true;
			};
		}

		private bool contractFailed;
	}
}
