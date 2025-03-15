using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Billy.PolishReceiptRecognitionAlgorithm.OcrJson
{
    public class ReceiptOcrParser
    {
        private readonly ISpatialAnalysis _spatialAnalysis;
        private readonly IOrientationFixAlgorithm _orientationFixAlgorithm;
        private readonly IArtifactsRemovalAlgorithm _artifactsRemovalAlgorithm;
        private readonly SpatialClaimsResolver<DetectionWithId> _spatialClaimsResolver;

        public ReceiptOcrParser(
            ISpatialAnalysis spatialAnalysis,
            IOrientationFixAlgorithm orientationFixAlgorithm,
            IArtifactsRemovalAlgorithm artifactsRemovalAlgorithm)
        {
            _spatialAnalysis = spatialAnalysis;
            _orientationFixAlgorithm = orientationFixAlgorithm;
            _artifactsRemovalAlgorithm = artifactsRemovalAlgorithm;

            _spatialClaimsResolver = new SpatialClaimsResolver<DetectionWithId>();
        }

        public Result GetLines(OcrResult ocrResult)
        {
            var ocrWithFixedOrientation = _orientationFixAlgorithm
                .FixOrientation(ocrResult);

            var ocrWithoutArtifacts = _artifactsRemovalAlgorithm
                .RemoveArtifacts(ocrWithFixedOrientation.TransformedOcr);

            var detections = ocrWithoutArtifacts
                .Detections
                .Select((detection, index) => new DetectionWithId(
                    id: index,
                    text: detection.Text,
                    box: detection.Box))
                .ToArray();

            var spatialClaims = _spatialAnalysis
                .GetSpatialClaims(detections);

            var lines = _spatialClaimsResolver.Resolve(
                detections,
                spatialClaims);

            var sortedFromLeftToRight = SortEachLineMembersLeftToRight(lines);
            var sortedFromTopToBottom = SortLinesTopToBottom(sortedFromLeftToRight);

            var actualLines = sortedFromTopToBottom
                .Select(MergeLines)
                .ToArray();

            return new Result(
                lines: actualLines,
                originalReceiptOrientationInRadians: ocrWithFixedOrientation.OriginalOrientationInRadians);
        }
        
        private static List<DetectionWithId[]> SortEachLineMembersLeftToRight(
            IEnumerable<Line<DetectionWithId>> lines)
        {
            return lines
                .Select(SortLineMembersLeftToRight)
                .ToList();

            DetectionWithId[] SortLineMembersLeftToRight(
                Line<DetectionWithId> line)
            {
                return line
                    .Items
                    .OrderBy(member => member.Box.LeftTop.X)
                    .ToArray();
            }
        }

        private static List<DetectionWithId[]> SortLinesTopToBottom(
            IEnumerable<DetectionWithId[]> lines)
        {
            return lines
                .OrderBy(GetMembersAverageYCoordinate)
                .ToList();

            double GetMembersAverageYCoordinate(
                IEnumerable<DetectionWithId> lineMembers)
            {
                return lineMembers
                    .SelectMany(member => new[]
                    {
                        member.Box.LeftTop.Y,
                        member.Box.RightTop.Y
                    })
                    .Average();
            }
        }

        private static ReceiptLine MergeLines(IList<DetectionWithId> lines)
        {
            var boundingBox = BoundingBox.GetBoundingBoxFor(
                lines.Select(line => line.Box));

            var text = string.Join(" ", lines.Select(line => line.Text.Trim()));

            return new ReceiptLine(
                text,
                boundingBox);
        }

        public class Result
        {
            public ReceiptLine[] Lines { get; }

            public double OriginalReceiptOrientationInRadians { get; }

            public Result(
                ReceiptLine[] lines,
                double originalReceiptOrientationInRadians)
            {
                Lines = lines;
                OriginalReceiptOrientationInRadians = originalReceiptOrientationInRadians;
            }
        }
    }
}