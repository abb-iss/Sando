using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sando.Core
{
	public static class WordSplitter
	{
		public static string[] Split(string word) 
		{
			word = CamelTypeToUnderscore(word);
			return SplitOnDelimiters(word);
		}

		private static string CamelTypeToUnderscore(string word)
		{
			return Regex.Replace(word, @"([A-Z][a-z]+|[A-Z]+)", "_$1").Replace(" _", "_");
		}

		private static string[] SplitOnDelimiters(string word) 
		{
			char[] delimiters = new char[] { '_' , ':' };
			return word.Split(delimiters,StringSplitOptions.RemoveEmptyEntries);
		}


	}
}
