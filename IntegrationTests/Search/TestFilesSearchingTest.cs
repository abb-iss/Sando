using System;
using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Snowball;
using NUnit.Framework;
using Sando.Core;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer;
using Sando.Indexer.Searching;
using Sando.Indexer.Searching.Criteria;
using Sando.SearchEngine;
using Sando.UI.Monitoring;
using UnitTestHelpers;
using Sando.Recommender;
using System.Threading;

namespace Sando.IntegrationTests.Search
{
	[TestFixture]
	public class TestFilesSearchingTest : AutomaticallyIndexingTestClass
	{
		[Test]
		public void FieldSearchWithUnderscore()
		{
            string keywords = "_solutionKey";
		    var expectedLowestRank = 2;
            Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Field && (el.ProgramElement.Name == "_solutionKey");
			EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
		}

        public override string GetIndexDirName()
        {
            return "TestFilesSearchingTest";
        }

        public override string GetFilesDirectory()
        {
            return "..\\..\\IntegrationTests\\TestFiles";
        }

        public override TimeSpan? GetTimeToCommit()
        {
            return TimeSpan.FromSeconds(4);
        }
        

	}
}
