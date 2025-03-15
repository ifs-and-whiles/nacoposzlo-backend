using System;
using System.Linq;

namespace Billy.PolishReceiptRecognitionAlgorithm.OcrJson
{
    public class ArtifactsRemovalAlgorithm : IArtifactsRemovalAlgorithm
    {
        private const double MaxAngleDeviation = Math.PI / 4;

        public OcrResult RemoveArtifacts(OcrResult ocrResult)
        {
            return RemoveVerticalBoxes(ocrResult);
        }

        private static OcrResult RemoveVerticalBoxes(OcrResult original)
        {
            var detections = original.Detections
                .Where(d => IsBoundingBoxHorizontal(d.Box))
                .ToArray();

            return new OcrResult(detections);
        }

        private static bool IsBoundingBoxHorizontal(BoundingBox box)
        {
            return Math.Abs(box.GetOrientationInRadians()) < MaxAngleDeviation;
        }
    }
}