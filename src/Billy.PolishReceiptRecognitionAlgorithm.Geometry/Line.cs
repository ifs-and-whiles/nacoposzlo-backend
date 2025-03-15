using System;
using System.Linq;
using System.Numerics;
using Billy.CodeReadability;

namespace Billy.PolishReceiptRecognitionAlgorithm.Geometry
{
    public abstract class Line
    {
        public abstract bool IsEqualTo(Line other);

        public abstract bool Contains(Point point);

        public abstract Either<Point, LinesRelation> TryIntersect(Line other);

        public abstract Line GetPerpendicularLine(Point point);

        public abstract double DistanceTo(Point point);

        public Point GetProjectionOf(Point point)
        {
            var perpendicularLine = GetPerpendicularLine(point);

            return TryIntersect(perpendicularLine).Match(
                intersection => intersection,
                linesRelation => throw new InvalidOperationException(
                    $"Line should always be intersected by other, perpendicular line - but found '{linesRelation}'"));
        }

        public static Line FromPoints(Point a, Point b)
        {
            if(Double.AreEqual(a.X, b.X))
                return new VerticalLine(a.X);

            return new NotVerticalLine(a, b);
        }

        public static Line ThroughOriginAtAngle(double radians)
        {
            if (Double.AreEqual(radians, Math.PI / 2) ||
                Double.AreEqual(radians, 3.0 * Math.PI / 2.0))
            {
                return Vertical(0);
            }

            return FromParameters(
                a: Math.Tan(radians),
                b: 0);
        }
        
        public static Line FromParameters(double a, double b) => 
            new NotVerticalLine(a, b);

        public static Line Vertical(double x) => 
            new VerticalLine(x);
        
        private class NotVerticalLine : Line
        {
            public double A { get; }
            public double B { get; }

            public NotVerticalLine(Point p1, Point p2)
            {
                A = (p2.Y - p1.Y) / (p2.X - p1.X);
                B = p1.Y - A * p1.X;
            }

            public NotVerticalLine(double a, double b)
            {
                A = a;
                B = b;
            }

            public override bool IsEqualTo(Line other)
            {
                switch (other)
                {
                    case null:
                        throw new ArgumentNullException(nameof(other));

                    case NotVerticalLine otherNotVertical:
                        return Double.AreEqual(A, otherNotVertical.A) && 
                               Double.AreEqual(B, otherNotVertical.B);
                    default:
                        return false;
                }
            }

            private bool IsParallelTo(Line other)
            {
                switch (other)
                {
                    case null:
                        throw new ArgumentNullException(nameof(other));

                    case NotVerticalLine otherNotVertical:
                        return Double.AreEqual(A, otherNotVertical.A);

                    default:
                        return false;
                }
            }

            public override bool Contains(Point point) =>
                Double.AreEqual(At(point.X).Y, point.Y);

            public override Either<Point, LinesRelation> TryIntersect(Line other)
            {
                if (other == null)
                    throw new ArgumentNullException(nameof(other));

                if (IsEqualTo(other))
                    return LinesRelation.SameLine;

                if (IsParallelTo(other))
                    return LinesRelation.ParallelLines;

                switch (other)
                {
                    case NotVerticalLine otherNotVertical:
                        return At((otherNotVertical.B - B) / (A - otherNotVertical.A));
                    
                    case VerticalLine verticalLine:
                        return At(verticalLine.X);

                    default:
                        throw new ArgumentException(
                            $"Line of type '{other.GetType()}' is not recognized");
                }
            }

            public override Line GetPerpendicularLine(Point point)
            {
                if (Double.AreEqual(A, 0)) return Vertical(point.X);

                var newA = -1 / A;
                var newB = point.Y + point.X / A;

                return new NotVerticalLine(newA, newB);
            }

            public override double DistanceTo(Point point) => 
                Math.Abs(-A * point.X + point.Y - B) / Math.Sqrt(A * A + 1);
            public Point At(double x) => new Point(x, A * x + B);

            public override string ToString()
            {
                return $"y = {A}x + {B}";
            }
        }

        private class VerticalLine : Line
        {
            public double X { get; }

            public VerticalLine(double x)
            {
                X = x;
            }

            public override bool IsEqualTo(Line other)
            {
                switch (other)
                {
                    case null:
                        throw new ArgumentNullException(nameof(other));

                    case VerticalLine otherVertical:
                        return Double.AreEqual(X, otherVertical.X);

                    default:
                        return false;
                }
            }

            public override bool Contains(Point point) =>
                Double.AreEqual(X, point.X);

            public override Either<Point, LinesRelation> TryIntersect(Line other)
            {
                switch (other)
                {
                    case null:
                        throw new ArgumentNullException(nameof(other));

                    case NotVerticalLine notVerticalLine:
                        return notVerticalLine.At(X);

                    case VerticalLine verticalLine:
                        return IsEqualTo(verticalLine) 
                            ? LinesRelation.SameLine
                            : LinesRelation.ParallelLines;

                    default:
                        throw new ArgumentException(
                            $"Line of type '{other.GetType()}' is not recognized");
                }
            }

            public override Line GetPerpendicularLine(Point point) => 
                new NotVerticalLine(0, point.Y);

            public override double DistanceTo(Point point) =>
                Math.Abs(X - point.X);
            
            public override string ToString()
            {
                return $"x = {X}";
            }
        }
    }
}