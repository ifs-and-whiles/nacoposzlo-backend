using System;
using System.Collections.Generic;
using System.Linq;
using Billy.PolishReceiptRecognitionAlgorithm.Geometry;

namespace Billy.PolishReceiptRecognitionAlgorithm.OcrJson
{
    public interface IOrientationFixAlgorithm
    {
        OcrResultWithFixedOrientation FixOrientation(OcrResult ocrResult);
    }


    public class AverageOrientationFixAlgorithm : IOrientationFixAlgorithm
    {
        public OcrResultWithFixedOrientation FixOrientation(
            OcrResult ocrResult)
        {
            var approximateOrientation = ocrResult
                .CalculateApproximateOrientation();

            var transformedOcr = ocrResult.Rotate(
                approximateOrientation);

            return new OcrResultWithFixedOrientation(
                originalOrientationInRadians: approximateOrientation,
                transformedOcr: transformedOcr);
        }
    }
}