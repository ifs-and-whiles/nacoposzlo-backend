using System;
using System.Collections.Generic;
using System.Linq;

namespace Billy.PolishReceiptRecognitionAlgorithm.OcrJson
{
    public class RaysHitCounterSpatialAnalysis : ISpatialAnalysis
    {
        public SpatialClaims<DetectionWithId> GetSpatialClaims(
            IReadOnlyList<DetectionWithId> detectionWithIds)
        {
            var detections = CalculateRangesForEachDetection(
                detectionWithIds);

            var yRange = CalculateWholeRange(
                detections.Select(d => d.YRange));

            var xRange = CalculateWholeRange(
                detections.Select(d => d.XRange));
            
            var yClusters = CalculateClusters(yRange, 5000);
            var xClusters = CalculateClusters(xRange, 100);

            var yClustersMap = BuildClustersMap(yClusters, detections, detection => detection.YRange);
            var xClustersMap = BuildClustersMap(xClusters, detections, detection => detection.XRange);

            return new SpatialClaims<DetectionWithId>(
                rowClaims: GetClaims(yClustersMap),
                columns: GetColumns(xClustersMap));
        }

        private IReadOnlyList<Detection> CalculateRangesForEachDetection(
            IReadOnlyList<DetectionWithId> detectionWithIds)
        {
            return detectionWithIds
                .Select((rawDetectedText, index) => new Detection (
                    yRange: CalculateSingleYRange(rawDetectedText),
                    xRange: CalculateSingleXRange(rawDetectedText),
                    item: rawDetectedText))
                .ToArray();
        }

        private Range CalculateWholeRange(IEnumerable<Range> ranges)
        {
            double? minY = null;
            double? maxY = null;

            foreach (var range in ranges)
            {
                if (minY == null)  minY = range.Min;
                else if (range.Min < minY) minY = range.Min;

                if (maxY == null) maxY = range.Max;
                else if (range.Max > maxY) maxY = range.Max;
            }

            return new Range(
                minY.Value,
                maxY.Value);
        }

        private Range CalculateSingleYRange(DetectionWithId detection)
        {
            var top = new[]
            {
                detection.Box.LeftTop.Y,
                detection.Box.RightTop.Y,
            }.Max();

            var bottom = new[]
            {
                detection.Box.LeftBottom.Y,
                detection.Box.RightBottom.Y
            }.Min();

            return new Range(
                top,
                bottom);
        }

        private Range CalculateSingleXRange(DetectionWithId detection)
        {
            var xs = new[]
            {
                detection.Box.LeftTop.X,
                detection.Box.RightTop.X,
                detection.Box.LeftBottom.X,
                detection.Box.RightBottom.X
            }.OrderBy(x => x).ToArray();

            return new Range(
                xs.First(),
                xs.Last());
        }

        private IEnumerable<Range> CalculateClusters(Range wholeRange, int clusterCount)
        {
            var delta = wholeRange.Length / clusterCount;

            for (var i = 0; i < clusterCount; i++)
            {
                yield return new Range(
                    wholeRange.Min + i * delta,
                    wholeRange.Min + (i + 1) * delta);
            }
        }

        private Dictionary<Range, List<Detection>> BuildClustersMap(
            IEnumerable<Range> clusters,
            IReadOnlyList<Detection> detections,
            Func<Detection, Range> getRangeFunc)
        {
            var clustersDict = new Dictionary<Range, List<Detection>>();

            foreach (var cluster in clusters)
            {
                var currentItems =  new List<Detection>();
                clustersDict.Add(cluster, currentItems);

                foreach (var detection in detections)
                {
                    var range = getRangeFunc(detection);

                    if (range.Overlaps(cluster))
                    {
                        currentItems.Add(detection);
                    }
                }
            }

            return clustersDict;
        }

        private IReadOnlyList<RowClaim<DetectionWithId>> GetClaims(
            Dictionary<Range, List<Detection>> clustersMap)
        {
            var claimsStrengthMap = new ClaimsStrengthMap();

            foreach (var sameClusterItems in clustersMap.Values)
            {
                var pairs = GetAllPairs(sameClusterItems);

                foreach (var pair in pairs)
                {
                    claimsStrengthMap.IncrementStrength(pair);
                }
            }

            return claimsStrengthMap.GetClaims();
        }

        //todo encapsulate in as more general combinations method
        private IEnumerable<Pair<Detection>> GetAllPairs(List<Detection> detections)
        {
            for (int i = 0; i < detections.Count; i++)
            {
                var a = detections[i];

                for (int j = i+1; j < detections.Count; j++)
                {
                    var b = detections[j];

                    yield return new Pair<Detection>(a, b);
                }
            }
        }

        private IReadOnlyList<Column<DetectionWithId>> GetColumns(
            Dictionary<Range, List<Detection>> xClustersMap)
        {
            return xClustersMap
                .Select(x => new Column<DetectionWithId>(
                    x.Value.Select(v => v.Item)))
                .ToArray();
        }

        private class ClaimsStrengthMap
        {
            private readonly Dictionary<Pair<Detection>, int> _map = new Dictionary<Pair<Detection>, int>();
            
            public void IncrementStrength(Pair<Detection> pair)
            {
                if (!_map.ContainsKey(pair))
                {
                    _map.Add(pair, 0);
                }

                _map[pair] = _map[pair] + 1;
            }

            public IReadOnlyList<RowClaim<DetectionWithId>> GetClaims()
            {
                return _map
                    .Select(kvp => new RowClaim<DetectionWithId>(
                        new Pair<DetectionWithId>(
                            kvp.Key.First.Item,
                            kvp.Key.Second.Item), 
                        kvp.Value))
                    .ToArray();
            }
        }

        private class Detection: IIdentifiable
        {
            public Detection(Range yRange, Range xRange, DetectionWithId item)
            {
                YRange = yRange;
                XRange = xRange;
                Item = item;
            }

            public Range YRange { get; }
            public Range XRange { get; }
            public DetectionWithId Item { get; }

            protected bool Equals(Detection other)
            {
                return Id == other.Id;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Detection)obj);
            }

            public override int GetHashCode()
            {
                return Id;
            }

            public override string ToString()
            {
                return Item.Text;
            }

            public int Id => Item.Id;
        }

        private class Range
        {
            public double Min { get; }
            
            public double Max { get; }

            public Range(double min, double max)
            {
                Min = min;
                Max = max;
                Length = max - min;
            }

            public double Length { get; }

            public bool Overlaps(Range other)
            {
                return Min <= other.Max && other.Min <= Max;
            }

            protected bool Equals(Range other)
            {
                return Min.Equals(other.Min) && Max.Equals(other.Max);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Min, Max);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Range) obj);
            }

            public override string ToString()
            {
                return $"{Min} - {Max}";
            }
        }
    }
}