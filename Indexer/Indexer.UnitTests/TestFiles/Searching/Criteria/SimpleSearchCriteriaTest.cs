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
		public void SimpleSearchCriteria_ExactMatchToQueryString()
		{
			SimpleSearchCriteria simpleSearchCriteria = new SimpleSearchCriteria();
            simpleSearchCriteria.SearchTerms.Add("ServiceLocator.Resolve<DTE2>();");
			string queryString = simpleSearchCriteria.ToQueryString();
            Assert.IsTrue(queryString.Contains("Source:*ServiceLocator\\.Resolve\\<DTE2\\>\\(\\)\\;*"), "Created query string is invalid!");
		}


        [Test]
        public void SimpleSearchCriteria_QuotedToQueryString()
        {
            SimpleSearchCriteria simpleSearchCriteria = new SimpleSearchCriteria();
            simpleSearchCriteria.SearchTerms.Add("\"ServiceLocator.Resolve<DTE2>();\"");
            string queryString = simpleSearchCriteria.ToQueryString();
            Assert.IsTrue(queryString.Contains("Source:*ServiceLocator\\.Resolve\\<DTE2\\>\\(\\)\\;*"), "Created query string is invalid!");
        }

        [Test]
        public void SimpleSearchCriteria_QuotedNoWeirdCharsToQueryString()
        {
            SimpleSearchCriteria simpleSearchCriteria = new SimpleSearchCriteria();
            simpleSearchCriteria.SearchTerms.Add("\"ServiceLocatorResolve\"");
            string queryString = simpleSearchCriteria.ToQueryString();
            Assert.IsTrue(queryString.Contains("Source:*ServiceLocatorResolve*"), "Created query string is invalid!");
        }

        [Test]
        public void SimpleSearchCriteria_QuotedWithSpaces()
        {
            SimpleSearchCriteria simpleSearchCriteria = new SimpleSearchCriteria();
            simpleSearchCriteria.SearchTerms.Add("\"foreach(var term in SearchTerms)\"");
            string queryString = simpleSearchCriteria.ToQueryString();
            Assert.IsTrue(queryString.Contains("Source:*foreach\\(var?term?in?SearchTerms\\)*"), "Created query string is invalid!");
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
			Assert.AreEqual(queryString, "(" + SandoField.AccessLevel.ToString() + ":private)", "Created query string is invalid!");
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
			Assert.AreEqual(queryString, "(" + SandoField.AccessLevel.ToString() + ":protected OR " + SandoField.AccessLevel.ToString() + ":public)", "Created query string is invalid!");
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
        public void SimpleSearchCriteria_ToQueryStringCreatesValidFileExtensionsQueryString_SingleCondition()
        {
            SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria()
                {
                    SearchByFileExtension = true,
                    FileExtensions = new SortedSet<string>
                        {
                            ".cs"
                        }
                };
            var queryString = simpleSearchCriteria.ToQueryString();
            Assert.AreEqual(queryString, "(" + SandoField.FileExtension.ToString() + ":\".cs\")", "Created query string is invalid!");
            try
            {
                var query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, SandoField.FileExtension.ToString(), new SimpleAnalyzer()).Parse(queryString);
                Assert.NotNull(query, "Generated query object is null!");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public void SimpleSearchCriteria_ToQueryStringCreatesFileExtensionsQueryString_MultipleConditions()
        {
            SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria()
                {
                    SearchByFileExtension = true,
                    FileExtensions = new SortedSet<string>()
                        {
                            ".cs",
                            ".h"
                        }
                };
            var queryString = simpleSearchCriteria.ToQueryString();
            Assert.AreEqual(queryString, "(" + SandoField.FileExtension.ToString() + ":\".cs\" OR " + SandoField.FileExtension.ToString() + ":\".h\")", "Created query string is invalid!");
            try
            {
                var query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, SandoField.FullFilePath.ToString(), new SimpleAnalyzer()).Parse(queryString);
                Assert.NotNull(query, "Generated query object is null!");
            }
            catch (Exception ex)
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
			Assert.AreEqual(queryString, "(" + SandoField.ProgramElementType.ToString() + ":class)", "Created query string is invalid!");
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
																					ProgramElementType.Enum
																				}
													};
			string queryString = simpleSearchCriteria.ToQueryString();
			Assert.AreEqual(queryString, "(" + SandoField.ProgramElementType.ToString() + ":property OR " + SandoField.ProgramElementType.ToString() + ":enum OR " + SandoField.ProgramElementType.ToString() + ":class)", "Created query string is invalid!");
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
            string actual = "(" + SandoField.Body.ToString() + ":SimpleClass OR " + SandoField.Name.ToString() + ":SimpleClass^2 OR " + SandoField.ExtendedClasses.ToString() + ":SimpleClass^0.5 OR " + SandoField.ImplementedInterfaces.ToString() + ":SimpleClass^0.5 OR " + SandoField.Arguments.ToString() + ":SimpleClass^0.25 OR " + SandoField.ReturnType.ToString() + ":SimpleClass^0.25 OR " + SandoField.Namespace.ToString() + ":SimpleClass^0.25 OR " + SandoField.DataType.ToString() + ":SimpleClass OR " + SandoField.Source.ToString() + ":SimpleClass OR " + SandoField.ClassName.ToString() + ":SimpleClass)";
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
			Assert.AreEqual(queryString, "(" + SandoField.ExtendedClasses.ToString() + ":SimpleClass^0.5)", "Created query string is invalid!");
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
			Assert.AreEqual(queryString, "(" + SandoField.Name.ToString() + ":SimpleClass^2 OR " + SandoField.ExtendedClasses.ToString() + ":SimpleClass^0.5 OR " + SandoField.Namespace.ToString() + ":SimpleClass^0.25)", "Created query string is invalid!");
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
		    string actual = "(" + SandoField.Name.ToString() + ":Class^2 OR " + SandoField.ExtendedClasses.ToString() + ":Class^0.5 OR " + SandoField.Namespace.ToString() + ":Class^0.25 OR " + "" + SandoField.Name.ToString() + ":Simple^2 OR " + SandoField.ExtendedClasses.ToString() + ":Simple^0.5 OR " + SandoField.Namespace.ToString() + ":Simple^0.25)";
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
			Assert.AreEqual(queryString, "(" + SandoField.Name.ToString() + ":*Class?Simple*^2)", "Created query string is invalid!");
			try
			{
                var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, SandoField.Name.ToString(), new SimpleAnalyzer());
                parser.SetAllowLeadingWildcard(true);
				Query query = parser.Parse(queryString);
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
                                                        SearchByFileExtension = true,
                                                        FileExtensions = new SortedSet<string>()
																	{
																		".cs",
																		".h"
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
																					ProgramElementType.Enum
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
			Assert.AreEqual(queryString, "(" + SandoField.AccessLevel.ToString() + ":protected OR " + SandoField.AccessLevel.ToString() + ":public) AND " +
										"(" + SandoField.ProgramElementType.ToString() + ":property OR " + SandoField.ProgramElementType.ToString() + ":enum OR " + SandoField.ProgramElementType.ToString() + ":class) AND " +
                                        "(" + SandoField.FileExtension.ToString() + ":\".cs\" OR " + SandoField.FileExtension.ToString() + ":\".h\") AND " +
                                        "(" + SandoField.FullFilePath.ToString() + ":\"C:/Project/*.cs\" OR " + SandoField.FullFilePath.ToString() + ":\"C:/Project2/*.cs\") AND " +
										"(" + SandoField.Name.ToString() + ":SimpleClass^2 OR " + SandoField.ExtendedClasses.ToString() + ":SimpleClass^0.5 OR " + SandoField.Namespace.ToString() + ":SimpleClass^0.25)", "Created query string is invalid!");
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
																			"\"+ - && || ! ( ) { } [ ] ^ ~ : \""
																		}
													};
			string queryString = simpleSearchCriteria.ToQueryString();
            Assert.AreEqual("(Body:*\\+?\\-?\\&\\&?\\|\\|?\\!?\\(?\\)?\\{?\\}?\\[?\\]?\\^?\\~?\\:?*)", queryString, "Created query string is invalid!");
			try
			{
                var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, SandoField.Name.ToString(), new SimpleAnalyzer());
                parser.SetAllowLeadingWildcard(true);
				Query query = parser.Parse(queryString);
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
