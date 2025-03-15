using System.Collections.Generic;
using System.Linq;

namespace Billy.CollectionTools
{
    public static class Arrays
    {
        public static T[] Concat<T>(params IEnumerable<T>[] arrays)
        {
            return  arrays
                .Where(array => array != null)
                .Aggregate(Enumerable.Empty<T>(), (acc, array) => acc.Concat(array))
                .ToArray();
        }
    }
}