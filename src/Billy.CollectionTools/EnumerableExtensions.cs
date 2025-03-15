using System;
using System.Collections.Generic;
using System.Linq;
using Billy.CodeReadability;

namespace Billy.CollectionTools
{
    public static class EnumerableExtensions
    {
        public static Option<T> TryGetFirst<T>(this IEnumerable<T> enumerable) =>
            !enumerable.Any() 
                ? Option<T>.None 
                : Option<T>.From(enumerable.First());

        public static Option<T> TryGetLast<T>(this IEnumerable<T> enumerable) =>
            Option<T>.From(enumerable.LastOrDefault());

        public static Option<T> TryGetFirst<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate) =>
            Option<T>.From(enumerable.FirstOrDefault(predicate));
    }
}