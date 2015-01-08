using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UAParser
{
    static class RegexBinderBuilder
    {
        public static Func<Match, IEnumerator<int>, TResult> SelectMany<T1, T2, TResult>(
            this Func<Match, IEnumerator<int>, T1> binder,
            Func<T1, Func<Match, IEnumerator<int>, T2>> continuation,
            Func<T1, T2, TResult> projection)
        {
            return (m, num) => { T1 f; return projection(f = binder(m, num), continuation(f)(m, num)); };
        }
    }
}