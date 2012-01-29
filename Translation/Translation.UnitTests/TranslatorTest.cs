using NUnit.Framework;
using Sando.Translation;

namespace Translation.UnitTests
{
	[TestFixture]
	public class TranslatorTest
	{
		[Test]
		public void Translator_GetTranslationReturnsValidTranslationForValidCode()
		{
			Assert.True(Translator.GetTranslation(TranslationCode.TestResource) == "Test resource");
		}
	}
}
