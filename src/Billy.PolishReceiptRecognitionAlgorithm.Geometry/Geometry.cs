using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Billy.PolishReceiptRecognitionAlgorithm.Geometry
{
    public class Geometry
    {
        public static Vector2 Vector(Point from, Point to)
        {
            return new Vector2(
                (float)(to.X - from.X),
                (float)(to.Y - from.Y));
        }

        public static Point PointInTheMiddle(Point first, Point second)
        {
            return new Point(
                x: PointInTheMiddleX(first, second),
                y: PointInTheMiddleY(first, second));
        }

        public static double DistanceBetween(Point first, Point second)
        {
            var x = second.X - first.X;
            var y = second.Y - first.Y;
            return Math.Sqrt(x * x + y * y);
        }

        public static double OrientationInRadians(Point first, Point second)
        {
            return Math.Atan2(
                y: second.Y - first.Y,
                x: second.X - first.X);
        }
        
        private static double PointInTheMiddleX(Point first, Point second)
        {
            return first.X + (second.X - first.X) / 2;
        }

        private static double PointInTheMiddleY(Point first, Point second)
        {
            return first.Y + (second.Y - first.Y) / 2;
        }
    }
}
