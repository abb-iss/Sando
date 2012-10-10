using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using NUnit.Framework;
using Sando.Core.Extensions;
using Sando.Core.Tools;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer.Documents;
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
			Assert.AreEqual(queryString, "(" + SandoField.AccessLevel.ToString() + ":Private)", "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, SandoField.AccessLevel.ToString(), new SimpleAnalyzer()).Parse(queryString);
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
			Assert.AreEqual(queryString, "(" + SandoField.AccessLevel.ToString() + ":Protected OR " + SandoField.AccessLevel.ToString() + ":Public)", "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, SandoField.AccessLevel.ToString(), new SimpleAnalyzer()).Parse(queryString);
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
			Assert.AreEqual(queryString, "(" + SandoField.FullFilePath.ToString() + ":\"C:/Project/*.cs\")", "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, SandoField.FullFilePath.ToString(), new SimpleAnalyzer()).Parse(queryString);
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
			Assert.AreEqual(queryString, "(" + SandoField.FullFilePath.ToString() + ":\"C:/Project/*.cs\" OR " + SandoField.FullFilePath.ToString() + ":\"C:/Project2/*.cs\")", "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, SandoField.FullFilePath.ToString(), new SimpleAnalyzer()).Parse(queryString);
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
			Assert.AreEqual(queryString, "(" + SandoField.ProgramElementType.ToString() + ":Class*)", "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, SandoField.ProgramElementType.ToString(), new SimpleAnalyzer()).Parse(queryString);
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
			Assert.AreEqual(queryString, "(" + SandoField.ProgramElementType.ToString() + ":Class* OR " + SandoField.ProgramElementType.ToString() + ":DocComment* OR " + SandoField.ProgramElementType.ToString() + ":Enum* OR " + SandoField.ProgramElementType.ToString() + ":Property*)", "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, SandoField.ProgramElementType.ToString(), new SimpleAnalyzer()).Parse(queryString);
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
		    string actual = "(" + SandoField.Body.ToString() + ":SimpleClass OR " + SandoField.Name.ToString() + ":SimpleClass^4 OR " + SandoField.Values.ToString() + ":SimpleClass OR " + SandoField.ExtendedClasses.ToString() + ":SimpleClass OR " + SandoField.ImplementedInterfaces.ToString() + ":SimpleClass OR " + SandoField.Arguments.ToString() + ":SimpleClass OR " + SandoField.ReturnType.ToString() + ":SimpleClass OR " + SandoField.Namespace.ToString() + ":SimpleClass OR " + SandoField.DataType.ToString() + ":SimpleClass)";
		    Assert.AreEqual(queryString, actual, "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, SandoField.Name.ToString(), new SimpleAnalyzer()).Parse(queryString);
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
			Assert.AreEqual(queryString, "(" + SandoField.ExtendedClasses.ToString() + ":SimpleClass)", "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, SandoField.ExtendedClasses.ToString(), new SimpleAnalyzer()).Parse(queryString);
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
			Assert.AreEqual(queryString, "(" + SandoField.Name.ToString() + ":SimpleClass^4 OR " + SandoField.ExtendedClasses.ToString() + ":SimpleClass OR " + SandoField.Namespace.ToString() + ":SimpleClass)", "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, SandoField.Name.ToString(), new SimpleAnalyzer()).Parse(queryString);
				Assert.NotNull(query, "Generated query object is null!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[Test]
		public void SimpleSearchCriteria_ToQueryStringCreatesValidUsageTypesQueryString_MultipleSearchTerms()
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
																			"Class",
																			"Simple"
																		}
			};
			string queryString = simpleSearchCriteria.ToQueryString();
		    string actual = "(" + SandoField.Name.ToString() + ":Class^4 OR " + SandoField.ExtendedClasses.ToString() + ":Class OR " + SandoField.Namespace.ToString() + ":Class OR " + "" + SandoField.Name.ToString() + ":Simple^4 OR " + SandoField.ExtendedClasses.ToString() + ":Simple OR " + SandoField.Namespace.ToString() + ":Simple)";
		    Assert.AreEqual(queryString, actual, "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, SandoField.Name.ToString(), new SimpleAnalyzer()).Parse(queryString);
				Assert.NotNull(query, "Generated query object is null!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[Test]
		public void SimpleSearchCriteria_ToQueryStringCreatesValidQueryString_QuotedSearchTerm()
		{
			SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria()
			{
				SearchByUsageType = true,
				UsageTypes = new SortedSet<UsageType>()
																		{
																			UsageType.Definitions
																		},
				SearchTerms = new SortedSet<string>(WordSplitter.ExtractSearchTerms("\"Class Simple\""))
			};
			string queryString = simpleSearchCriteria.ToQueryString();
			Assert.AreEqual(queryString, "(" + SandoField.Name.ToString() + ":\"class simple\"^4)", "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, SandoField.Name.ToString(), new SimpleAnalyzer()).Parse(queryString);
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
			Assert.AreEqual(queryString, "(" + SandoField.AccessLevel.ToString() + ":Protected OR " + SandoField.AccessLevel.ToString() + ":Public) AND " +
										"(" + SandoField.ProgramElementType.ToString() + ":Class* OR " + SandoField.ProgramElementType.ToString() + ":DocComment* OR " + SandoField.ProgramElementType.ToString() + ":Enum* OR " + SandoField.ProgramElementType.ToString() + ":Property*) AND " +
										"(" + SandoField.FullFilePath.ToString() + ":\"C:/Project/*.cs\" OR " + SandoField.FullFilePath.ToString() + ":\"C:/Project2/*.cs\") AND " +
										"(" + SandoField.Name.ToString() + ":SimpleClass^4 OR " + SandoField.ExtendedClasses.ToString() + ":SimpleClass OR " + SandoField.Namespace.ToString() + ":SimpleClass)", "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, SandoField.Name.ToString(), new SimpleAnalyzer()).Parse(queryString);
				Assert.NotNull(query, "Generated query object is null!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

        [Test]
		public void SimpleSearchCriteria_ToQueryStringCreatesValidQueryString_SpecialCharacters()
		{
			SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria()
													{
                                                        SearchByUsageType = true,
                                                        UsageTypes = new SortedSet<UsageType>()
																		{
																			UsageType.Bodies
																		},
														SearchTerms = new SortedSet<string>()
																		{
																			"+ - && || ! ( ) { } [ ] ^ \" ~ : \\"
																		}
													};
			string queryString = simpleSearchCriteria.ToQueryString();
            Assert.AreEqual("(Body:\"\\+ \\- \\&\\& \\|\\| \\! \\( \\) \\{ \\} \\[ \\] \\^ \\\" \\~ \\: \\\\\")", queryString, "Created query string is invalid!");
			try
			{
				Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, SandoField.Name.ToString(), new SimpleAnalyzer()).Parse(queryString);
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
            ExtensionPointsRepository extensionPointsRepository = ExtensionPointsRepository.Instance;
            extensionPointsRepository.RegisterWordSplitterImplementation(new WordSplitter());
            extensionPointsRepository.RegisterQueryWeightsSupplierImplementation(new QueryWeightsSupplier());
            extensionPointsRepository.RegisterQueryRewriterImplementation(new DefaultQueryRewriter());        
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
