using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Sando.ExperimentalExtensions.PaiceStemmer
{
	[TestFixture]
	public class PaiceStemmerTest
	{	
		[Test]
		public void StemmingTest()
		{
			string ruleDir = Environment.CurrentDirectory + "\\..\\..\\LIBS\\paice";
			PaiceStemmer paice = new PaiceStemmer(ruleDir, "");
			Assert.AreEqual(paice.stripAffixes("intercoastal"), "intercoast");
			Assert.AreEqual(paice.stripAffixes("scientists"), "scy");
			Assert.AreEqual(paice.stripAffixes("representing"), "repres");
		}

	}
}
