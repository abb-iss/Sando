using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Sando.ExperimentalExtensions.SpellChecking
{
    [TestFixture]
    public class SpellCheckingQueryRewriterTest
    {

        [TestCase]
        public void TestRewrite()
        {
            var spellChecka = new SpellCheckingQueryRewriter();
            var rewritten = spellChecka.RewriteQuery("dogz and catz");
            Assert.IsTrue(rewritten.Contains("dogs"));
        }

    }
}
