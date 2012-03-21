using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using NUnit.Framework;
using Sando.Core;
using Sando.Indexer.Searching;
using Sando.Indexer.Searching.Criteria;

namespace Sando.Indexer.UnitTests.Searching.Criteria
{
    [TestFixture]
	public class SimpleSearchCriteriaTest
	{
		[Test]
		public void SimpleSearchCriteria_ToQueryStringCreatesValidQueryStringForEmptySearchCriteria()
		{
			SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria();
			string queryString = simpleSearchCriteria.ToQueryString();
			Assert.AreEqual(queryString, String.Empty, "Created query string is invalid!");
		}

		[Test]
		public void SimpleSearchCriteria_ToQueryStringThrowsWhenSearchingByAccessLevelWithNoAccessLevelCriteria()
		{
			SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria()
			{
				SearchByAccessLevel = true
			};
			try
			{
				string queryString = simpleSearchCriteria.ToQueryString();
			}
			catch
			{
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void SimpleSearchCriteria_ToQueryStringThrowsWhenSearchingByLocationWithNoLocationCriteria()
		{
			SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria()
			{
				SearchByLocation = true
			};
			try
			{
				string queryString = simpleSearchCriteria.ToQueryString();
			}
			catch
			{
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void SimpleSearchCriteria_ToQueryStringThrowsWhenSearchingByProgramElementTypeWithNoProgramElementTypeCriteria()
		{
			SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria()
			{
				SearchByProgramElementType = true
			};
			try
			{
				string queryString = simpleSearchCriteria.ToQueryString();
			}
			catch
			{
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void SimpleSearchCriteria_ToQueryStringThrowsWhenSearchingByUsageTypeWithNoUsageTypeCriteria()
		{
			SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria()
			{
				SearchByUsageType = true
			};
			try
			{
				string queryString = simpleSearchCriteria.ToQueryString();
			}
			catch
			{
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void SimpleSearchCriteria_ToQueryStringCreatesValidAccessLevelsQueryString_SingleCondition()
		{
			SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria()
													{
														SearchByAccessLevel = true,
														AccessLevels = new SortedSet<AccessLevel>()
																		{
																			AccessLevel.Private
																		}
													};
			string queryString = simpleSearchCriteria.ToQueryString();
			Assert.AreEqual(queryString, "(AccessLevel:Private)", "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "AccessLevel", new SimpleAnalyzer()).Parse(queryString);
				Assert.NotNull(query, "Generated query object is null!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[Test]
		public void SimpleSearchCriteria_ToQueryStringCreatesValidAccessLevelsQueryString_MultipleConditions()
		{
			SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria()
													{
														SearchByAccessLevel = true,
														AccessLevels = new SortedSet<AccessLevel>()
																		{
																			AccessLevel.Public,
																			AccessLevel.Protected
																		}
													};
			string queryString = simpleSearchCriteria.ToQueryString();
			Assert.AreEqual(queryString, "(AccessLevel:Protected OR AccessLevel:Public)", "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "AccessLevel", new SimpleAnalyzer()).Parse(queryString);
				Assert.NotNull(query, "Generated query object is null!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[Test]
		public void SimpleSearchCriteria_ToQueryStringCreatesValidLocationsQueryString_SingleCondition()
		{
			SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria()
													{
														SearchByLocation = true,
														Locations = new SortedSet<string>()
																	{
																		"C:/Project/*.cs"
																	}
													};
			string queryString = simpleSearchCriteria.ToQueryString();
			Assert.AreEqual(queryString, "(FullFilePath:\"C:/Project/*.cs\")", "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "FullFilePath", new SimpleAnalyzer()).Parse(queryString);
				Assert.NotNull(query, "Generated query object is null!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[Test]
		public void SimpleSearchCriteria_ToQueryStringCreatesValidLocationsQueryString_MultipleConditions()
		{
			SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria()
													{
														SearchByLocation = true,
														Locations = new SortedSet<string>()
																	{
																		"C:/Project/*.cs",
																		"C:/Project2/*.cs"
																	}
													};
			string queryString = simpleSearchCriteria.ToQueryString();
			Assert.AreEqual(queryString, "(FullFilePath:\"C:/Project/*.cs\" OR FullFilePath:\"C:/Project2/*.cs\")", "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "FullFilePath", new SimpleAnalyzer()).Parse(queryString);
				Assert.NotNull(query, "Generated query object is null!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[Test]
		public void SimpleSearchCriteria_ToQueryStringCreatesValidProgramElementTypesQueryString_SingleCondition()
		{
			SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria()
													{
														SearchByProgramElementType = true,
														ProgramElementTypes = new SortedSet<ProgramElementType>()
																				{
																					ProgramElementType.Class
																				}
													};
			string queryString = simpleSearchCriteria.ToQueryString();
			Assert.AreEqual(queryString, "(ProgramElementType:Class)", "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "ProgramElementType", new SimpleAnalyzer()).Parse(queryString);
				Assert.NotNull(query, "Generated query object is null!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[Test]
		public void SimpleSearchCriteria_ToQueryStringCreatesValidProgramElementTypesQueryString_MultipleConditions()
		{
			SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria()
													{
														SearchByProgramElementType = true,
														ProgramElementTypes = new SortedSet<ProgramElementType>()
																				{
																					ProgramElementType.Property,
																					ProgramElementType.Class,
																					ProgramElementType.Enum,
																					ProgramElementType.DocComment
																				}
													};
			string queryString = simpleSearchCriteria.ToQueryString();
			Assert.AreEqual(queryString, "(ProgramElementType:Class OR ProgramElementType:DocComment OR ProgramElementType:Enum OR ProgramElementType:Property)", "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "ProgramElementType", new SimpleAnalyzer()).Parse(queryString);
				Assert.NotNull(query, "Generated query object is null!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[Test]
		public void SimpleSearchCriteria_ToQueryStringCreatesValidUsageTypesQueryString_NoCondition()
		{
			SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria()
													{
														SearchByUsageType = false,
														SearchTerms = new SortedSet<string>()
																		{
																			"SimpleClass"
																		}
													};
			string queryString = simpleSearchCriteria.ToQueryString();
			Assert.AreEqual(queryString, "((Body:SimpleClass) OR (Name:SimpleClass) OR (Values:SimpleClass) OR (ExtendedClasses:SimpleClass) OR (ImplementedInterfaces:SimpleClass) OR (Arguments:SimpleClass) OR (ReturnType:SimpleClass) OR (Namespace:SimpleClass) OR (DataType:SimpleClass))", "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "Name", new SimpleAnalyzer()).Parse(queryString);
				Assert.NotNull(query, "Generated query object is null!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[Test]
		public void SimpleSearchCriteria_ToQueryStringCreatesValidUsageTypesQueryString_SingleCondition()
		{
			SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria()
													{
														SearchByUsageType = true,
														UsageTypes = new SortedSet<UsageType>()
																		{
																			UsageType.ExtendedClasses
																		},
														SearchTerms = new SortedSet<string>()
																		{
																			"SimpleClass"
																		}
													};
			string queryString = simpleSearchCriteria.ToQueryString();
			Assert.AreEqual(queryString, "(ExtendedClasses:SimpleClass)", "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "ExtendedClasses", new SimpleAnalyzer()).Parse(queryString);
				Assert.NotNull(query, "Generated query object is null!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[Test]
		public void SimpleSearchCriteria_ToQueryStringCreatesValidUsageTypesQueryString_MultipleConditions()
		{
			SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria()
													{
														SearchByUsageType = true,
														UsageTypes = new SortedSet<UsageType>()
																		{
																			UsageType.ExtendedClasses,
																			UsageType.Definitions,
																			UsageType.NamespaceNames
																		},
														SearchTerms = new SortedSet<string>()
																		{
																			"SimpleClass"
																		}
													};
			string queryString = simpleSearchCriteria.ToQueryString();
			Assert.AreEqual(queryString, "(Name:SimpleClass OR ExtendedClasses:SimpleClass OR Namespace:SimpleClass)", "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "Name", new SimpleAnalyzer()).Parse(queryString);
				Assert.NotNull(query, "Generated query object is null!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[Test]
		public void SimpleSearchCriteria_ToQueryStringCreatesValidQueryString_AllConditions()
		{
			SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria()
													{
														SearchByAccessLevel = true,
														AccessLevels = new SortedSet<AccessLevel>()
																		{
																			AccessLevel.Public,
																			AccessLevel.Protected
																		},
														SearchByLocation = true,
														Locations = new SortedSet<string>()
																	{
																		"C:/Project/*.cs",
																		"C:/Project2/*.cs"
																	},
														SearchByProgramElementType = true,
														ProgramElementTypes = new SortedSet<ProgramElementType>()
																				{
																					ProgramElementType.Property,
																					ProgramElementType.Class,
																					ProgramElementType.Enum,
																					ProgramElementType.DocComment
																				},
														SearchByUsageType = true,
														UsageTypes = new SortedSet<UsageType>()
																		{
																			UsageType.ExtendedClasses,
																			UsageType.Definitions,
																			UsageType.NamespaceNames
																		},
														SearchTerms = new SortedSet<string>()
																		{
																			"SimpleClass"
																		}
													};
			string queryString = simpleSearchCriteria.ToQueryString();
			Assert.AreEqual(queryString, "(AccessLevel:Protected OR AccessLevel:Public) AND " +
										"(ProgramElementType:Class OR ProgramElementType:DocComment OR ProgramElementType:Enum OR ProgramElementType:Property) AND " +
										"(FullFilePath:\"C:/Project/*.cs\" OR FullFilePath:\"C:/Project2/*.cs\") AND " +
										"(Name:SimpleClass OR ExtendedClasses:SimpleClass OR Namespace:SimpleClass)", "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "Name", new SimpleAnalyzer()).Parse(queryString);
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
