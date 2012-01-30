using System;
using Sando.Core.Exceptions;
using Sando.Translation;

namespace Sando.Parser
{
	public class ParserException : SandoException
	{
		public ParserException(TranslationCode translationCode, String exceptionMessageFormatArg)
			: base(translationCode, null, exceptionMessageFormatArg)
		{
		}
	}
}
