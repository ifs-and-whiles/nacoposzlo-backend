using System.Collections.Generic;
using System.Linq;
using Billy.CodeReadability;

namespace Billy.PolishReceiptRecognitionAlgorithm.OcrJson
{
    public class Line<T>
    {
        public Line(IReadOnlyList<T> items)
        {
            Items = items;
        }

        public IReadOnlyList<T> Items { get; }

        public override string ToString()
        {
            return string.Join(" ", Items);
        }
    }

    public class SpatialClaimsResolver<T> where T:IIdentifiable
    {
        public Line<T>[] Resolve(
            IReadOnlyList<T> detections,
            SpatialClaims<T> claims)
        {
            var filteredClaims = FilterClaims(detections, claims);

            return MergeLines(detections, filteredClaims);
        }

        private static SpatialClaims<T> FilterClaims(
            IReadOnlyList<T> detections, 
            SpatialClaims<T> spatialClaims)
        {
            if (!spatialClaims.RowClaims.Any())
                return spatialClaims;

            var filteredClaims = FilterOutOverlappingOnSameColumn(spatialClaims);
            filteredClaims = FilterOutTooWeakClaims(filteredClaims);
            filteredClaims = FilterOutConflictedClaims(detections, filteredClaims);

            return filteredClaims;
        }

        private Line<T>[] MergeLines(IReadOnlyList<T> detections, SpatialClaims<T> filteredClaims)
        {
            var linesBuilder = LinesBuilder.Empty();

            foreach (var detection in detections)
            {
                if(linesBuilder.Contains(detection)) continue;

                var relatedClaims = GetRelatedClaims(
                    filteredClaims, 
                    detection);

                if (relatedClaims.Any())
                {
                    var items = relatedClaims
                        .SelectMany(claim => claim.Items.All)
                        .ToArray();

                    linesBuilder.AddToLine(items);
                }
                else
                {
                    linesBuilder.AddToLine(new[]
                    {
                        detection
                    });
                }
            }

            return linesBuilder.Build();
        }

        private static SpatialClaims<T> FilterOutTooWeakClaims(
            SpatialClaims<T> spatialClaims)
        {
            if (!spatialClaims.RowClaims.Any())
                return spatialClaims;

            var avgStrength = spatialClaims
                .RowClaims
                .Average(c => c.Strength);

            //we assume that everything which has strength lower than avg/3 is 
            //a result of an overlapping - and we just want to get rid of those
            var minAllowedStrength = avgStrength / 2;

            var sameLineClaims = spatialClaims
                .RowClaims
                .Where(claim => claim.Strength > minAllowedStrength)
                .ToArray();

            return spatialClaims.CopyWithRowClaims(
                sameLineClaims);
        }

        private static SpatialClaims<T> FilterOutOverlappingOnSameColumn(
            SpatialClaims<T> spatialClaims)
        {
            var sameLineClaims = spatialClaims
                .RowClaims
                .Where(claim => !spatialClaims.AreInSameColumn(
                    claim.Items.First,
                    claim.Items.Second))
                .ToArray();
            
            return spatialClaims.CopyWithRowClaims(
                sameLineClaims);
        }

        private static SpatialClaims<T> FilterOutConflictedClaims(
            IReadOnlyList<T> detections,
            SpatialClaims<T> spatialClaims)
        {
            var deletedClaims = new HashSet<RowClaim<T>>();

            foreach (var detection in detections)
            {
                var relatedClaims = GetRelatedClaims(
                    spatialClaims, 
                    detection);
                
                var strongestClaim = relatedClaims
                    .Max(claim => claim, claim => claim.Strength);

                foreach (var relatedClaim in relatedClaims)
                {
                    if (strongestClaim.Equals(relatedClaim)) continue;

                    if (spatialClaims.AreClaimsInConflict(strongestClaim, relatedClaim))
                    {
                        deletedClaims.Add(relatedClaim);
                    }
                }
            }

            var sameLineClaims = spatialClaims
                .RowClaims
                .Where(claim => !deletedClaims.Contains(claim))
                .ToArray();

            return spatialClaims.CopyWithRowClaims(
                sameLineClaims);
        }

        private static RowClaim<T>[] GetRelatedClaims(SpatialClaims<T> spatialClaims, T detection)
        {
            return spatialClaims
                .RowClaims
                .Where(claim => claim.Items.Contains(detection))
                .ToArray();
        }

        private class LinesBuilder
        {
            private readonly List<LineBuilder> _lineBuilders;

            private LinesBuilder()
            {
                _lineBuilders = new List<LineBuilder>();
            }

            public static LinesBuilder Empty()
            {
                return new LinesBuilder();
            }

            public void AddToLine(IEnumerable<T> items)
            {
                var lineBuilder = _lineBuilders
                    .FirstOrDefault(builder => builder.ContainsAny(items));

                if (lineBuilder == null)
                {
                    lineBuilder = LineBuilder.Empty();
                    _lineBuilders.Add(lineBuilder);
                }

                lineBuilder.Add(items);
            }

            public bool Contains(T item)
            {
                return _lineBuilders.Any(builder => builder.Contains(item));
            }

            public Line<T>[] Build()
            {
                return _lineBuilders
                    .Select(builder => new Line<T>(
                        builder.Items))
                    .ToArray();
            }
        }

        private class LineBuilder
        {
            private readonly HashSet<T> _items;

            public T[] Items => _items.ToArray();

            private LineBuilder()
            {
                _items = new HashSet<T>(
                    new IdentifiableEqualityComparer<T>());
            }

            public static LineBuilder Empty()
            {
                return new LineBuilder();
            }

            public bool Contains(T item)
            {
                return _items.Contains(item);
            }

            public bool ContainsAny(IEnumerable<T> items)
            {
                return items.Any(Contains);
            }

            public void Add(IEnumerable<T> items)
            {
                foreach (var item in items)
                {
                    _items.Add(item);
                }
            }
        }
    }
}
