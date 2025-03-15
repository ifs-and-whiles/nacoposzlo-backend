using System.Diagnostics;

namespace Billy.PolishReceiptRecognitionAlgorithm.OcrJson
{
    public class NumericallyImprovedRotationFixAlgorithm : IOrientationFixAlgorithm
    {
        private const double NumericalOptimizationScope = 0.174533; //10 degrees in radians
        private const double WidthOptimizationThreshold = 0.0001;
        private const int MaxIterations = 100;

        public OcrResultWithFixedOrientation FixOrientation(
            OcrResult ocrResult)
        {
            var approximateOrientation = ocrResult
                .CalculateApproximateOrientation();

            var goldenSectionSearch = new GoldenSectionSearch(
                WidthOptimizationThreshold,
                MaxIterations);

            var improvedRotation = goldenSectionSearch.Minimize(
                function: (rotation) => ocrResult
                    .Rotate(rotation)
                    .GetWidth(),
                lowerBound: approximateOrientation - NumericalOptimizationScope,
                upperBound: approximateOrientation + NumericalOptimizationScope);

            var transformedOcr = ocrResult.Rotate(
                improvedRotation);

            return new OcrResultWithFixedOrientation(
                originalOrientationInRadians: improvedRotation,
                transformedOcr: transformedOcr);
        }
    }
}