using System;
using System.Text.RegularExpressions;
using Sando.ExtensionContracts.SplitterContracts;

namespace Sando.TestExtensionPoints
{
	public class TestWordSplitter : IWordSplitter
	{
		public string[] ExtractWords(string text)
		{
			text = Regex.Replace(text, @"([A-Z][a-z]+|[A-Z]+|[0-9]+)", "_$1").Replace(" _", "_");
			char[] delimiters = new char[] { '_', ':' };
			return text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
		}
	}
}
