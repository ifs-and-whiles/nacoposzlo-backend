using System.Collections.Generic;

namespace Billy.PolishReceiptRecognitionAlgorithm.OcrJson
{
    public interface ISpatialAnalysis
    {
       SpatialClaims<DetectionWithId> GetSpatialClaims(
            IReadOnlyList<DetectionWithId> detectionWithIds);
    }
}