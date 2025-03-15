using System;

namespace Billy.PolishReceiptRecognitionAlgorithm.Geometry
{
    public static class Double
    {
        public const double Tolerance = 0.000000001;

        public static bool AreEqual(double first, double second)
        {
            return Math.Abs(first - second) < Tolerance;
        }
    }
}