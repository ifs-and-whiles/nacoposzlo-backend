

using System.Collections.Generic;
using System.Linq;
using Billy.PolishReceiptRecognitionAlgorithm.Geometry;

namespace Billy.PolishReceiptRecognitionAlgorithm.OcrJson
{
    public class BoundingBox
    {
        public Point LeftTop { get; }
        public Point RightTop { get; }

        public Point LeftBottom { get; }
        public Point RightBottom { get; }

        public BoundingBox(Point leftTop, Point rightTop, Point rightBottom, Point leftBottom)
        {
            LeftTop = leftTop;
            RightTop = rightTop;
            RightBottom = rightBottom;
            LeftBottom = leftBottom;
        }
        
        public override string ToString()
        {
            return $"LT: {LeftTop} RB: {RightBottom}";
        }

        public static BoundingBox GetBoundingBoxFor(IEnumerable<BoundingBox> boxes)
        {
            var boxesList = boxes as IList<BoundingBox> ?? boxes.ToArray();

            var leftX = GetLeftX(boxesList);
            var rightX = GetRightX(boxesList);
            var bottomY = GetBottomY(boxesList);
            var topY = GetTopY(boxesList);

            return new BoundingBox(
                leftTop: new Point(leftX, topY),
                rightTop: new Point(rightX, topY),
                rightBottom: new Point(rightX, bottomY),
                leftBottom: new Point(leftX, bottomY));
        }

        private static double GetTopY(IEnumerable<BoundingBox> boxes)
        {
            return boxes
                .SelectMany(box => new[]
                {
                    box.LeftTop.Y,
                    box.RightTop.Y
                })
                .Min();
        }

        private static double GetBottomY(IEnumerable<BoundingBox> boxes)
        {
            return boxes
                .SelectMany(box => new[]
                {
                    box.LeftBottom.Y,
                    box.RightBottom.Y
                })
                .Max();
        }

        private static double GetRightX(IEnumerable<BoundingBox> boxes)
        {
            return boxes
                .SelectMany(box => new[]
                {
                    box.RightBottom.X,
                    box.RightTop.X
                })
                .Max();
        }

        private static double GetLeftX(IEnumerable<BoundingBox> boxes)
        {
            return boxes
                .SelectMany(box => new[]
                {
                    box.LeftBottom.X,
                    box.LeftTop.X
                })
                .Min();
        }
    }

    public static class BoundingBoxExtensions
    {
        public static double GetOrientationInRadians(this BoundingBox box)
        {
            var leftMiddle = Geometry.Geometry.PointInTheMiddle(
                first: box.LeftTop,
                second: box.LeftBottom);

            var rightMiddle = Geometry.Geometry.PointInTheMiddle(
                first: box.RightTop,
                second: box.RightBottom);

            var orientation = Geometry.Geometry.OrientationInRadians(
                first: leftMiddle,
                second: rightMiddle);

            return orientation;
        }
        
        public static BoundingBox Rotate(
            this BoundingBox box,
            double angleInRadians)
        {
            return new BoundingBox(
                leftTop: box.LeftTop.Rotate(angleInRadians),
                rightTop: box.RightTop.Rotate(angleInRadians),
                rightBottom: box.RightBottom.Rotate(angleInRadians),
                leftBottom: box.LeftBottom.Rotate(angleInRadians));
        }

        public static double GetMinX(
            this BoundingBox box)
        {
            var minX = box.LeftBottom.X;

            if (box.LeftTop.X < minX)
                minX = box.LeftTop.X;

            if (box.RightTop.X < minX)
                minX = box.RightTop.X;

            if (box.RightBottom.X < minX)
                minX = box.RightBottom.X;

            return minX;
        }

        public static double GetMaxX(
            this BoundingBox box)
        {
            var maxX = box.LeftBottom.X;

            if (box.LeftTop.X > maxX)
                maxX = box.LeftTop.X;

            if (box.RightTop.X > maxX)
                maxX = box.RightTop.X;

            if (box.RightBottom.X > maxX)
                maxX = box.RightBottom.X;

            return maxX;
        }
    }
}