using System;
using Sando.Core.Extensions;
using Sando.Core.Tools;
using Sando.ExtensionContracts.SplitterContracts;

namespace Sando.Indexer.Documents
{
	public static class SandoDocumentStringExtension
	{
		public static string ToSandoSearchable(this String fieldValue)
		{
			if(String.IsNullOrWhiteSpace(fieldValue))
				return fieldValue;
		    IWordSplitter wordSplitterImplementation = ExtensionPointsRepository.Instance.GetWordSplitterImplementation();
		    string splitWords;
            if (wordSplitterImplementation != null)
            {
                splitWords = String.Join(" ", wordSplitterImplementation.ExtractWords(fieldValue));
            }
            else
            {
                //For testing, should never happend during execution
                splitWords = String.Join(" ",(new WordSplitter()).ExtractWords(fieldValue));
            }
			if(splitWords == fieldValue)
				return fieldValue;
			string result = fieldValue + Delimiter + splitWords;
			return result;
		}

		public static string ToSandoDisplayable(this String fieldValue)
		{
			if(String.IsNullOrWhiteSpace(fieldValue))
				return fieldValue;
			if(fieldValue.IndexOf(Delimiter) < 0)
				return fieldValue;
			string result = fieldValue.Substring(0, fieldValue.IndexOf(Delimiter));
			return result;
		}

	    public const string Delimiter = " ✉∞ ";
	}
}
