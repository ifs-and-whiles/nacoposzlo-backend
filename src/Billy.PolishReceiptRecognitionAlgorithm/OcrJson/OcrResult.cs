using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Billy.PolishReceiptRecognitionAlgorithm.OcrJson
{
    public class OcrResult
    {
        public IReadOnlyList<RawDetectedText> Detections { get; }

        public OcrResult(
            IEnumerable<RawDetectedText> detections)
        {
            Detections = detections.ToArray();
        }
    }

    public static class OcrResultExtensions
    {
        private const double MaxAngleDeviation = Math.PI / 4;

        public static double CalculateApproximateOrientation(
            this OcrResult ocrResult)
        {
            var detectionsWithOrientation = ocrResult
                .Detections
                .Select(detection => new
                {
                    Detection = detection,
                    Orientation = detection.Box.GetOrientationInRadians(),
                })
                .ToArray();

            var referentialOrientation = detectionsWithOrientation
                .Average(d => d.Orientation);

            //it happens that OCR contains detections in with very strange orientation,
            //for example perpendicular to all the rest - we need to get rid of them

            var acceptableDetections = detectionsWithOrientation
                .Where(detection => Math.Abs(detection.Orientation - referentialOrientation) < MaxAngleDeviation)
                .ToArray();

            var finalAngle = acceptableDetections
                .Average(d => d.Orientation);

            return finalAngle;
        }

        public static OcrResult Rotate(
            this OcrResult original, 
            double orientationInRadians)
        {
            var detections = original.Detections.Select(originalDetection => new RawDetectedText
            {
                Text = originalDetection.Text,
                Box = originalDetection.Box.Rotate(-1 * orientationInRadians)
            });

            return new OcrResult(
                detections: detections);
        }

        /// <summary>
        /// returns the distance between the minimum x coordinate and maximum x coordinate
        /// of printed text recognition result detections
        /// </summary>
        /// <param name="ocrResult"></param>
        /// <returns></returns>
        public static double GetWidth(this OcrResult ocrResult)
        {
            double? minX = null;
            double? maxX = null;

            if (ocrResult.Detections.Count == 0)
                return 0;

            foreach (var detection in ocrResult.Detections)
            {
                var currentMinX = detection.Box.GetMinX();
                var currentMaxX = detection.Box.GetMaxX();

                if (minX == null) minX = currentMinX;
                else if (currentMinX < minX) minX = currentMinX;

                if (maxX == null) maxX = currentMaxX;
                else if (currentMaxX > maxX) maxX = currentMaxX;
            }

            return maxX.Value - minX.Value;
        }
    }
}