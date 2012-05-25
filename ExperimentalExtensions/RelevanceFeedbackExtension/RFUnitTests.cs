using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.ExperimentalExtensions.RelevanceFeedbackExtension
{
	[TestFixture]
	public class RFUnitTests
	{
		[Test]
		public void RFTrainingEntryTest()
		{
			List<IMetric> metrics = new List<IMetric>();
			metrics.Add(new QueryFileNameCosine());
			metrics.Add(new QueryElementNameCosine());
			List<CodeSearchResult> sandoResults = new List<CodeSearchResult>();
			CodeSearchResult a = new CodeSearchResult(new CommentElement("a", 1, "abc.cs", "a", "a"), 1);
			CodeSearchResult b = new CodeSearchResult(new CommentElement("b", 1, "abc.cs", "b", "b"), 1);
			CodeSearchResult c = new CodeSearchResult(new CommentElement("c", 1, "abc.cs", "c", "c"), 1);
			sandoResults.Add(a);
			sandoResults.Add(b);
			sandoResults.Add(c);
			RFTrainingEntry rfEntry = new RFTrainingEntry(1, "a c abc",sandoResults, metrics);

			rfEntry.AddRelevance(c);
			Assert.AreEqual(rfEntry.IsComplete(), true);

			string serialization = "#query 1" + Environment.NewLine +
								   "2 qid:1 1:0.41 2:0.58" + Environment.NewLine +  //"c" - since it was clicked
								   "1 qid:1 1:0.41 2:0.58" + Environment.NewLine +  //"a" 
								   "1 qid:1 1:0.41 2:0" + Environment.NewLine;      //"b"
			Assert.AreEqual(rfEntry.SerializeInClickedOrder(), serialization);
		}

		[Test]
		public void RFMetric1Test()
		{
			CodeSearchResult result = new CodeSearchResult(new CommentElement("a", 1, "a\\b\\c\\abc.cs", "a", "a"), 1);
			string query = "a b c d e";

			QueryFileNameCosine cosMetric = new QueryFileNameCosine();
			double m = cosMetric.runMetric(query,result);
			Assert.AreEqual(Math.Round(m,2), 0.60);
		}

		[Test]
		public void RFMetric2Test()
		{
			CodeSearchResult result = new CodeSearchResult(new CommentElement("AaBbCcFf", 1, "abc.cs", "a", "a"), 1);
			string query = "aa bb cc dd ee";

			QueryElementNameCosine cosMetric = new QueryElementNameCosine();
			double m = cosMetric.runMetric(query, result);
			Assert.AreEqual(Math.Round(m, 2), 0.67);
		}

		[Test]
		public void RFReorderResultsTest()
		{
			RelevanceFeedbackExtension rfe = new RelevanceFeedbackExtension();

			String ranking = "1" + Environment.NewLine + "2" + Environment.NewLine + "3";
			String rankFile = Environment.CurrentDirectory + "\\rankfile.dat";
			System.IO.File.WriteAllText(rankFile, ranking);

			List<CodeSearchResult> sandoResults = new List<CodeSearchResult>();
			CodeSearchResult four = new CodeSearchResult(new CommentElement("four", 1, "abc.cs", "a", "a"), 4);
			CodeSearchResult five = new CodeSearchResult(new CommentElement("five", 1, "abc.cs", "a", "a"), 5);
			CodeSearchResult six = new CodeSearchResult(new CommentElement("six", 1, "abc.cs", "a", "a"), 6);
			sandoResults.Add(four);
			sandoResults.Add(five);
			sandoResults.Add(six);
			Assert.AreEqual(sandoResults.ElementAt(0), four);
			Assert.AreEqual(sandoResults.ElementAt(1), five);
			Assert.AreEqual(sandoResults.ElementAt(2), six);

			IQueryable<CodeSearchResult> rerankedResults = rfe.RerankResults(sandoResults.AsQueryable(), rankFile);
			Assert.AreEqual(rerankedResults.ElementAt(0), six);
			Assert.AreEqual(rerankedResults.ElementAt(1), five);
			Assert.AreEqual(rerankedResults.ElementAt(2), four);
		}

		[Test]
		public void RFTheWholeShabangTest()
		{
			RelevanceFeedbackExtension rfe = new RelevanceFeedbackExtension(1);

			System.IO.File.Delete(rfe.TrainingFile);
			
			//1. query arrives
			rfe.RewriteQuery("query two");

			//2. results are returned by Sando
			List<CodeSearchResult> sandoResults = new List<CodeSearchResult>();
			CodeSearchResult one = new CodeSearchResult(new CommentElement("one", 1, "one\\abc.cs", "a", "a"), 1);
			CodeSearchResult two = new CodeSearchResult(new CommentElement("two", 1, "two\\abc.cs", "b", "b"), 2);
			sandoResults.Add(one);
			sandoResults.Add(two);
			rfe.ReorderSearchResults(sandoResults.AsQueryable());

			//3. click happens (has to be not on the first element)
			rfe.NotifySelection(two);

			//4. another set of query and results arrive, and now they should be getting reranked
			rfe.RewriteQuery("query five");
			sandoResults.Clear();
			CodeSearchResult four = new CodeSearchResult(new CommentElement("four", 1, "four\\abc.cs", "a", "a"), 4);
			CodeSearchResult five = new CodeSearchResult(new CommentElement("five", 1, "five\\abc.cs", "a", "a"), 5);
			sandoResults.Add(four);
			sandoResults.Add(five);
			IQueryable<CodeSearchResult> rerankedResults = rfe.ReorderSearchResults(sandoResults.AsQueryable());
			Assert.AreEqual(rfe.OpMode, RFMode.Operating);

			//5. now the results should be reordered
			Assert.AreEqual(rerankedResults.ElementAt(0), five);
			Assert.AreEqual(rerankedResults.ElementAt(1), four);
		}

	}
}
