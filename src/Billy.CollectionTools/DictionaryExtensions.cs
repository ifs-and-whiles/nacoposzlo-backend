using System.Collections.Generic;
using Billy.CodeReadability;

namespace Billy.CollectionTools
{
    public static class DictionaryExtensions
    {
        public static Option<TValue> TryGet<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.ContainsKey(key) ? Option<TValue>.Some(dictionary[key]) : Option<TValue>.None;
        }
    }
}