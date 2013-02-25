using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABB.SrcML;
using NUnit.Framework;
using System.Xml.Linq;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace LocalSearch.UnitTests
{
    [TestFixture]
    class ContextTest
    {
        [Test]
        public void GetRelationTest1()
        {
            ProgramElement pe1 = new MethodElement("DatabaseMenuCommands", 7, "", "", AccessLevel.Public, "", "", "", System.Guid.NewGuid(), "", "", true);
            ProgramElement pe2 = new FieldElement("DatabaseCommand", 12, "", "", AccessLevel.Public, "", System.Guid.NewGuid(), "", "", "");
            CodeSearchResult cs1 = new CodeSearchResult(pe1, 1.0);
            CodeSearchResult cs2 = new CodeSearchResult(pe2, 1.0);

            Context obj = new Context();
            obj.Intialize(@"..\..\Local Search\LocalSearch.UnitTests\TestFiles\DatabaseMenuCommands.cs");
            List<int> lines = new List<int>();

            ProgramElementRelation res1 = obj.GetRelation(cs1, cs2, ref lines);
            ProgramElementRelation res2 = obj.GetRelation(cs2, cs1, ref lines);

            Assert.AreEqual(res1, ProgramElementRelation.Use);
            Assert.AreEqual(res2, ProgramElementRelation.UseBy);

            Assert.AreEqual(lines.Count, 1);
            Assert.AreEqual(lines.ElementAt(0), 9);                        
        }

        [Test]
        public void GetRelationTest2()
        {
            ProgramElement pe1 = new MethodElement("genMultiSelectImp", 371, "", "", AccessLevel.Private, "", "", "", System.Guid.NewGuid(), "", "", true);
            ProgramElement pe2 = new MethodElement("genConfigImp", 104, "", "", AccessLevel.Public, "", "", "", System.Guid.NewGuid(), "", "", true);
            ProgramElement pe3 = new FieldElement("allSelections", 71, "", "", AccessLevel.Private, "", System.Guid.NewGuid(), "", "", "");
            CodeSearchResult cs1 = new CodeSearchResult(pe1, 1.0);
            CodeSearchResult cs2 = new CodeSearchResult(pe2, 1.0);
            CodeSearchResult cs3 = new CodeSearchResult(pe3, 1.0);

            Context obj = new Context();
            obj.Intialize(@"..\..\Local Search\LocalSearch.UnitTests\TestFiles\ConfigManip.cs");
            List<int> lines1 = new List<int>();
            List<int> lines2 = new List<int>();

            ProgramElementRelation res1 = obj.GetRelation(cs1, cs2, ref lines1);
            ProgramElementRelation res2 = obj.GetRelation(cs3, cs1, ref lines2);

            Assert.AreEqual(res1, ProgramElementRelation.CallBy);
            Assert.AreEqual(res2, ProgramElementRelation.UseBy);

            Assert.AreEqual(lines1.Count, 1);
            Assert.AreEqual(lines1.ElementAt(0), 115);

            Assert.AreEqual(lines2.Count, 3);
            Assert.AreEqual(lines2.ElementAt(0), 380);
            Assert.AreEqual(lines2.ElementAt(1), 384);
            Assert.AreEqual(lines2.ElementAt(2), 391);
        }

        [Test]
        public void LevenshteinDistanceTest()
        {
            String a = "kitten";
            String b = "sitting";
            Context obj = new Context();
            int distance = obj.LevenshteinDistance(a,b);
            Assert.AreEqual(distance, 3);
        }
    }
}
