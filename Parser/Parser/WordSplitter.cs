using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sando.Parser
{
	public static class WordSplitter
	{
		public static string[] split(string word) 
		{
			List<string> splits = new List<string>();

			word = camelTypeToUnderscore(word);

			if(word.Contains("_"))
			{
				splits.AddRange(SplitUnderscores(word).ToList());
			}

			return splits.ToArray();
		}

		private static string camelTypeToUnderscore(string word)
		{
			return Regex.Replace(word, @"([A-Z][a-z]+)", "_$1");
		}

		private static string[] SplitUnderscores(string word) 
		{
			char[] delimiters = new char[] { '_' };
			return word.Split(delimiters,StringSplitOptions.RemoveEmptyEntries);
		}


	}
}
