using System;
using System.Collections;
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

        public static IEnumerable<T> DistinctBy<T, M>(this IEnumerable<T> list,
            Func<T, M> selector) where M : IEquatable<M>
        {
            var set = new HashSet<T>(list, new GenericEqualityCompare<T,M>(selector));
            return set.AsEnumerable();
        }

        private class GenericEqualityCompare<T, M> : IEqualityComparer<T>
            where M : IEquatable<M>
        {
            private readonly Func<T, M> selector;

            public GenericEqualityCompare(Func<T, M> selector)
            {
                this.selector = selector;
            }

            public bool Equals(T x, T y)
            {
                var mx = selector.Invoke(x);
                var my = selector.Invoke(y);
                return mx.Equals(my);
            }

            public int GetHashCode(T obj)
            {
                return 0;
            }
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}
