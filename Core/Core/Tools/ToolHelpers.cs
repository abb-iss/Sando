using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.Core.QueryRefomers;

namespace Sando.Core.Tools
{
    public class ToolHelpers
    {
        public static IEnumerable<T> RemoveRedundance<T>(IEnumerable<T> list) where T : IEquatable<T>
        {
            return list.Distinct();
        }

        public static IEnumerable<ReformedWord> CreateNonChangedTerm(string word)
        {
            return new[] { new ReformedWord(TermChangeCategory.NOT_CHANGED, word, word, String.Empty) };
        }
    }
}
