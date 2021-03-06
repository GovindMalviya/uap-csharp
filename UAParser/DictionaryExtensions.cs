﻿using System;
using System.Collections.Generic;

namespace UAParser
{
    static class DictionaryExtensions
    {
        public static TValue Find<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary == null) throw new ArgumentNullException("dictionary");
            TValue result;
            return dictionary.TryGetValue(key, out result) ? result : default(TValue);
        }
    }
}