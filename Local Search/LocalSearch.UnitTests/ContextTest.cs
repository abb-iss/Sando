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
        private static String srcPath = @"..\..\Local Search\LocalSearch.UnitTests\TestFiles\DatabaseMenuCommands.cs";
        
        [Test]
        public void GetRelationTest1()
        {
            ProgramElement pe1 = new MethodElement("DatabaseMenuCommands", 7, "", "", AccessLevel.Public, "", "", "", System.Guid.NewGuid(), "", "", true);
            ProgramElement pe2 = new FieldElement("DatabaseCommand", 12, "", "", AccessLevel.Public, "", System.Guid.NewGuid(), "", "", "");
            CodeSearchResult cs1 = new CodeSearchResult(pe1, 1.0);
            CodeSearchResult cs2 = new CodeSearchResult(pe2, 1.0);

            Context obj = new Context(srcPath);
            List<int> lines = new List<int>();

            ProgramElementRelation res1 = obj.GetRelation(cs1, cs2, ref lines);
            ProgramElementRelation res2 = obj.GetRelation(cs2, cs1, ref lines);

            Assert.AreEqual(res1, ProgramElementRelation.Use);
            Assert.AreEqual(res2, ProgramElementRelation.UseBy);

            Assert.AreEqual(lines.Count, 2);
            Assert.AreEqual(lines.ElementAt(0), 9);
            Assert.AreEqual(lines.ElementAt(1), 9);            
        }
    }
}
