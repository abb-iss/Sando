using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.Core.QueryRefomers;

namespace Sando.Core.Tools
{
    public static class ToolHelpers
    {
        public static IEnumerable<T> RemoveRedundance<T>(this IEnumerable<T> list) where T : IEquatable<T>
        {
            return list.Distinct();
        }

        public static IEnumerable<ReformedWord> CreateNonChangedTerm(string word)
        {
            return new[] { new ReformedWord(TermChangeCategory.NOT_CHANGED, word, word, String.Empty) };
        }

        public static IEnumerable<T> TrimIfOverlyLong<T>(this IEnumerable<T> list, int maxCount)
        {
            var newList = list.ToList();
            return newList.Count() > maxCount ? newList.GetRange(0, maxCount) : newList;
        }
    }
}
