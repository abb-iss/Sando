using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using NUnit.Framework;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer.Documents;
using Sando.Indexer.Searching.Criteria;

namespace Sando.Indexer.UnitTests.Searching.Criteria
{
	[TestFixture]
	public class NotSearchCriteriaTest
	{
		[Test]
		public void NotSearchCriteria_ConstructorThrowsWhenFirstSearchCriteriaObjectIsNull()
		{
			try
			{
				NotSearchCriteria notSearchCriteria = new NotSearchCriteria(null, new SimpleSearchCriteria());
			}
			catch
			{
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void NotSearchCriteria_ConstructorThrowsWhenSecondSearchCriteriaObjectIsNull()
		{
			try
			{
				NotSearchCriteria notSearchCriteria = new NotSearchCriteria(new SimpleSearchCriteria(), null);
			}
			catch
			{
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void NotSearchCriteria_ToQueryStringCreatesValidQueryStringForValidData()
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
				SearchCriteria notSearchCriteria = new NotSearchCriteria(simpleSearchCriteria1, simpleSearchCriteria2);
				string queryString = notSearchCriteria.ToQueryString();
				Assert.AreEqual(queryString, "(" + SandoField.ProgramElementType.ToString() + ":Class) NOT (" + SandoField.FullFilePath.ToString() + ":\"C:/Project/*.cs\")", "Created query string is invalid!");
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, SandoField.ProgramElementType.ToString(), new SimpleAnalyzer()).Parse(queryString);
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
