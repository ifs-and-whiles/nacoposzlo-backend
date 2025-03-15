using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace Billy.PolishReceiptRecognitionAlgorithm.OcrJson
{
    public class SpatialClaims<T> where T : IIdentifiable
    {
        public IReadOnlyList<RowClaim<T>> RowClaims { get; }

        public IReadOnlyList<Column<T>> Columns { get; }

        private readonly IReadOnlyDictionary<T, int> _columnSpans;
        
        public SpatialClaims(
            IReadOnlyList<RowClaim<T>> rowClaims, 
            IReadOnlyList<Column<T>> columns)
        {
            RowClaims = rowClaims;
            Columns = columns;

            _columnSpans = CalculateColumnSpans(columns);
        }

        public SpatialClaims(
            IReadOnlyList<RowClaim<T>> rowClaims,
            IReadOnlyList<Column<T>> columns, 
            IReadOnlyDictionary<T, int> columnSpans)
        {
            RowClaims = rowClaims;
            Columns = columns;
            _columnSpans = columnSpans;
        }

        private Dictionary<T, int> CalculateColumnSpans(IReadOnlyList<Column<T>> columns)
        {
            var dict = new Dictionary<T, int>(
                new IdentifiableEqualityComparer<T>());

            foreach (var column in columns)
            {
                foreach (var item in column.Items)
                {
                    if (!dict.ContainsKey(item))
                    {
                        dict.Add(item, 0);
                    }

                    dict[item] = dict[item] + 1;
                }
            }

            return dict;
        }


        public bool AreInSameColumn(T first, T second)
        {
            var minSpan = Math.Min(
                GetColumnSpan(first),
                GetColumnSpan(second));
            
            if (minSpan > 1)
            {
                return Columns.Count(column => column.ContainsAll(first, second)) > 1;
            } 

            return Columns.Any(
                column => column.ContainsAll(first, second));
        }

        private int GetColumnSpan(T item)
        {
            return _columnSpans.ContainsKey(item)
                ? _columnSpans[item]
                : 0;
        }

        public bool AreClaimsInConflict(RowClaim<T> first, RowClaim<T> second)
        {
            return AreItemsInConflict(first.Items.First, second.Items.First)
                   || AreItemsInConflict(first.Items.First, second.Items.Second)
                   || AreItemsInConflict(first.Items.Second, second.Items.First)
                   || AreItemsInConflict(first.Items.Second, second.Items.Second);
        }

        private bool AreItemsInConflict(T first, T second)
        {
            return first.Id != second.Id && AreInSameColumn(first, second);
        }

        public SpatialClaims<T> CopyWithRowClaims(IEnumerable<RowClaim<T>> rowClaims)
        {
            return new SpatialClaims<T>(
                rowClaims.ToArray(),
                Columns,
                _columnSpans);
        }
    }
}