namespace Sando.Translation
{
	public static class Translator
	{
		public static string GetTranslation(TranslationCode translationCode)
		{
			return Translations.ResourceManager.GetString(translationCode.ToString());
		}
	}
}
