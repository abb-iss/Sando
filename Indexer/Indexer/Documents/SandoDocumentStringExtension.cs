using System;
using Sando.Core.Tools;

namespace Sando.Indexer.Documents
{
	public static class SandoDocumentStringExtension
	{
		public static string ToSandoSearchable(this String fieldValue)
		{
			if(String.IsNullOrWhiteSpace(fieldValue))
				return fieldValue;
			string splitWords = String.Join(" ", WordSplitter.ExtractWords(fieldValue));
			if(splitWords == fieldValue)
				return fieldValue;
			string result = fieldValue + delimiter + splitWords;
			return result;
		}

		public static string ToSandoDisplayable(this String fieldValue)
		{
			if(String.IsNullOrWhiteSpace(fieldValue))
				return fieldValue;
			if(fieldValue.IndexOf(delimiter) < 0)
				return fieldValue;
			string result = fieldValue.Substring(0, fieldValue.IndexOf(delimiter));
			return result;
		}

		private static string delimiter = "#";
	}
}
