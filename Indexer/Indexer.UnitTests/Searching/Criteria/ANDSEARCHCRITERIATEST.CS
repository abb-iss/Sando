using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using NUnit.Framework;
using Sando.Core;
using Sando.Indexer.Searching.Criteria;

namespace Sando.Indexer.UnitTests.Searching.Criteria
{
	[TestFixture]
	public class AndSearchCriteriaTest
	{
		[Test]
		public void AndSearchCriteria_ConstructorThrowsWhenFirstSearchCriteriaObjectIsNull()
		{
			try
			{
				AndSearchCriteria andSearchCriteria = new AndSearchCriteria(null, new SimpleSearchCriteria());
			}
			catch
			{
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void AndSearchCriteria_ConstructorThrowsWhenSecondSearchCriteriaObjectIsNull()
		{
			try
			{
				AndSearchCriteria andSearchCriteria = new AndSearchCriteria(new SimpleSearchCriteria(), null);
			}
			catch
			{
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void AndSearchCriteria_ToQueryStringCreatesValidQueryStringForValidData()
		{
			SearchCriteria simpleSearchCriteria1 = new SimpleSearchCriteria()
													{
														SearchByProgramElementType = true,
														ProgramElementTypes = new SortedSet<ProgramElementType>()
																				{
																					ProgramElementType.Class
																				}
													};
			SearchCriteria simpleSearchCriteria2 = new SimpleSearchCriteria()
													{
														SearchByLocation = true,
														Locations = new SortedSet<string>()
																	{
																		"C:/Project/*.cs"
																	}
													};
			try
			{
				SearchCriteria andSearchCriteria = new AndSearchCriteria(simpleSearchCriteria1, simpleSearchCriteria2);
				string queryString = andSearchCriteria.ToQueryString();
				Assert.AreEqual(queryString, "(ProgramElementType:Class) AND (FullFilePath:\"C:/Project/*.cs\")", "Created query string is invalid!");
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "ProgramElementType", new SimpleAnalyzer()).Parse(queryString);
				Assert.NotNull(query, "Generated query object is null!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[SetUp]
		public void resetContract()
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
