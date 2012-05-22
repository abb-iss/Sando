using System;
using NUnit.Framework;

namespace Configuration.OptionsPages
{
	[TestFixture]
	public class SandoOptionsTest
	{
		[Test]
		public void SandoOptions_ConstructorDoesNotThrowForNullValues()
		{
			try
			{
				SandoOptions sandoOptions = new SandoOptions(null, null);
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[Test]
		public void SandoOptions_ConstructorInitializesDefaultNumberOfSearchResultsReturned()
		{
			SandoOptions sandoOptions = new SandoOptions(null, null);
			Assert.AreEqual(20, sandoOptions.NumberOfSearchResultsReturned);
		}
	}
}
