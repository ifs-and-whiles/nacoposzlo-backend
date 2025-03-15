using System;

namespace Billy.PolishReceiptRecognitionAlgorithm.Geometry
{
    public struct Point
    {
        public double X { get; }
        public double Y { get; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        } 
    }

    public static class PointExtensions
    {
        public static Point Rotate(this Point point, double angleInRadians)
        {
            return new Point(
                x: point.X * Math.Cos(angleInRadians) - point.Y * Math.Sin(angleInRadians),
                y: point.X * Math.Sin(angleInRadians) + point.Y * Math.Cos(angleInRadians));
        }
    }
}
