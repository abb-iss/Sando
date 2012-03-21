using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using NUnit.Framework;
using Sando.Core;
using Sando.Indexer.Documents;
using Sando.Indexer.Searching.Criteria;

namespace Sando.Indexer.UnitTests.Searching.Criteria
{
	[TestFixture]
	public class ParenthesisSearchCriteriaTest
	{
		[Test]
		public void ParenthesisSearchCriteria_ConstructorThrowsWhenSearchCriteriaObjectIsNull()
		{
			try
			{
				ParenthesisSearchCriteria andSearchCriteria = new ParenthesisSearchCriteria(null);
			}
			catch
			{
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void ParenthesisSearchCriteria_ToQueryStringCreatesValidQueryStringForValidData()
		{
			SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria()
													{
														SearchByProgramElementType = true,
														ProgramElementTypes = new SortedSet<ProgramElementType>()
																				{
																					ProgramElementType.Class
																				}
													};
			try
			{
				SearchCriteria andSearchCriteria = new ParenthesisSearchCriteria(simpleSearchCriteria);
				string queryString = andSearchCriteria.ToQueryString();
				Assert.AreEqual(queryString, "((" + SandoField.ProgramElementType.ToString() + ":Class))", "Created query string is invalid!");
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
