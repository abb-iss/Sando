using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sando.Core.Extensions.PairedInterleaving;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Core.UnitTests.Extensions.PairedInterleaving
{
    [TestFixture]
    class PairedInterleavingTest
    {
        private PairedInterleavingManager manager;
        private List<CodeSearchResult> sandoResults;
        private List<CodeSearchResult> lexResults;
        private List<CodeSearchResult> interleavedResults;
        private List<int> clickIdx;

        [TestFixtureSetUp]
        public void SetupExperiment()
        {
            CodeSearchResult a = new CodeSearchResult(new CommentElement("a", 1, "abcd.cs", "a", "a"), 1);
            CodeSearchResult b = new CodeSearchResult(new CommentElement("b", 1, "abcd.cs", "b", "b"), 1);
            CodeSearchResult c = new CodeSearchResult(new CommentElement("c", 1, "abcd.cs", "c", "c"), 1);
            CodeSearchResult d = new CodeSearchResult(new CommentElement("d", 1, "abcd.cs", "d", "d"), 1);

            sandoResults = new List<CodeSearchResult>();
            sandoResults.Add(a); sandoResults.Add(b); sandoResults.Add(c);

            lexResults = new List<CodeSearchResult>();
            lexResults.Add(b); lexResults.Add(d); lexResults.Add(c);
            
            manager = new PairedInterleavingManager();
            interleavedResults = manager.BalancedInterleave(sandoResults, lexResults);

            clickIdx = new List<int>();
            clickIdx.Add(interleavedResults.IndexOf(b));
            clickIdx.Add(interleavedResults.IndexOf(c));
        }

        [Test]
        public void TestBalancedInterleaving()
        {
            List<CodeSearchResult> interleaving = manager.BalancedInterleave(sandoResults, lexResults);
            Assert.AreEqual(interleaving.Count, 4);
            if (interleaving[0] == sandoResults[0])
            {
                //b,d,c
                Assert.IsTrue(interleaving[1] == lexResults[0]);
                Assert.IsTrue(interleaving[2] == lexResults[1]);
                Assert.IsTrue(interleaving[3] == sandoResults[2]);
            }
            else if (interleaving[0] == lexResults[0])
            {
                //a,d,c
                Assert.IsTrue(interleaving[1] == sandoResults[0]);
                Assert.IsTrue(interleaving[2] == lexResults[1]);
                Assert.IsTrue(interleaving[3] == sandoResults[2]);
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void TestDetermineWinner()
        {
            int scoreA, scoreB;
            manager.DetermineWinner(sandoResults, lexResults, interleavedResults, 
                                        clickIdx, out scoreA, out scoreB);
            Assert.AreEqual(scoreA, 0);
            Assert.AreEqual(scoreB, 1);
        }


    }
}
