using System;
using Sando.Core.Exceptions;
using Sando.Translation;

namespace Sando.Indexer.Exceptions
{
	public class IndexerException : SandoException
	{
		public IndexerException(TranslationCode translationCode, Exception innerException)
			: base(translationCode, innerException)
		{
		}

		public IndexerException(TranslationCode translationCode, Exception innerException, object exceptionMessageFormatArg)
			: base(translationCode, innerException, exceptionMessageFormatArg)
		{
		}

		public IndexerException(TranslationCode translationCode, Exception innerException, object[] exceptionMessageFormatArgs)
			: base(translationCode, innerException, exceptionMessageFormatArgs)
		{
		}
	}
}
