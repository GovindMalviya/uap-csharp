using System;

namespace UAParser
{
    static class StringExtensions
    {
        public static string ReplaceFirstOccurence(this string input, string search, string replacement)
        {
            if (input == null) throw new ArgumentNullException("input");
            var index = input.IndexOf(search, StringComparison.Ordinal);
            return index >= 0
                ? input.Substring(0, index) + replacement + input.Substring(index + search.Length)
                : input;
        }
    }
}