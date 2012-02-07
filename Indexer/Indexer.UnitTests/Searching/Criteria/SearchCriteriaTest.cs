using System.Collections.Generic;
using System.Diagnostics.Contracts;
using NUnit.Framework;
using Sando.Core;
using Sando.Indexer.Searching.Criteria;

namespace Sando.Indexer.UnitTests
{
    [TestFixture]
	public class SearchCriteriaTest
	{
		//TODO update tests when the ToQueryStringMethod is ready

    	[Test]
		public void SearchCriteria_EqualsReturnTrueWhenObjectIsComparedToItsOwn()
		{
			SearchCriteria simpleSearchCriteria = new SimpleSearchCriteria()
													{
														SearchByProgramElementType = true,
														ProgramElementTypes = new SortedSet<ProgramElementType>() 
																				{
																					ProgramElementType.Class,
																					ProgramElementType.Method
																				}
													};
			Assert.True(simpleSearchCriteria.Equals(simpleSearchCriteria), "Equals should return true when search criteria object is compared to its own!");
		}

		[Test]
		public void SearchCriteria_EqualsReturnTrueWhenObjectsHaveTheSameData()
		{
			SearchCriteria simpleSearchCriteria1 = new SimpleSearchCriteria()
													{
														SearchByProgramElementType = true,
														ProgramElementTypes = new SortedSet<ProgramElementType>() 
																				{
																					ProgramElementType.Class,
																					ProgramElementType.Method
																				}
													}; 
			SearchCriteria simpleSearchCriteria2 = new SimpleSearchCriteria()
													{
														SearchByProgramElementType = true,
														ProgramElementTypes = new SortedSet<ProgramElementType>() 
																				{
																					ProgramElementType.Method,
																					ProgramElementType.Class
																				}
													};
			Assert.True(simpleSearchCriteria1.Equals(simpleSearchCriteria2), "Equals should return true when search criteria objects have the same data!");
		}

		[Test]
		public void SearchCriteria_EqualsReturnFalseWhenObjectsHaveDifferentData()
		{
			SearchCriteria simpleSearchCriteria1 = new SimpleSearchCriteria()
													{
														SearchByProgramElementType = true,
														ProgramElementTypes = new SortedSet<ProgramElementType>() 
																				{
																					ProgramElementType.Class,
																					ProgramElementType.Property
																				}
													}; 
			SearchCriteria simpleSearchCriteria2 = new SimpleSearchCriteria()
													{
														SearchByProgramElementType = true,
														ProgramElementTypes = new SortedSet<ProgramElementType>() 
																				{
																					ProgramElementType.Class,
																					ProgramElementType.Method
																				}
													};
			Assert.False(simpleSearchCriteria1.Equals(simpleSearchCriteria2), "Equals should return false when search criteria objects have different data!");
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
