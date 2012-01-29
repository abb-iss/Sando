using System;
using Sando.Translation;

namespace Sando.Core.Exceptions
{
	public abstract class SandoException : Exception
	{
		public SandoException(TranslationCode translationCode, Exception innerException)
			: base(Translator.GetTranslation(translationCode), innerException)
		{
		}

		public SandoException(TranslationCode translationCode, Exception innerException, object exceptionMessageFormatArg)
			: base(String.Format(Translator.GetTranslation(translationCode), exceptionMessageFormatArg), innerException)
		{
		}

		public SandoException(TranslationCode translationCode, Exception innerException, object[] exceptionMessageFormatArgs)
			: base(String.Format(Translator.GetTranslation(translationCode), exceptionMessageFormatArgs), innerException)
		{
		}
	}
}
