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
            Assert.IsTrue(rewritten.Contains("dog"));
        }

    }
}
