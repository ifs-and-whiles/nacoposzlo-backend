using System;
using System.Collections.Generic;
using System.Linq;

namespace Billy.PolishReceiptRecognitionAlgorithm.OcrJson
{
    public class Column<T> where T : IIdentifiable
    {
        private readonly HashSet<T> _items;

        public T[] Items => _items.ToArray();

        public Column(IEnumerable<T> items)
        {
            _items = new HashSet<T>(
                items,
                new IdentifiableEqualityComparer<T>());
        }

        public bool ContainsAll(params T[] items)
        {
            return items.All(_items.Contains);
        }

        public bool IsSubsetOf(Column<T> other)
        {
            return _items.IsSubsetOf(other._items);
        }

        public override string ToString()
        {
            return string.Join(", ", _items.OrderBy(x => x.Id));
        }
    }
}