namespace Billy.Receipts.Domain
{
    public class ReceiptPoint
    {
        public AbsolutePoint GetAbsolute() => new AbsolutePoint(
            _normalizedPoint.X * _receiptDimensions.ImageWidth,
            _normalizedPoint.Y * _receiptDimensions.ImageHeight);

        public NormalizedPoint GetNormalized() => _normalizedPoint;

        private readonly NormalizedPoint _normalizedPoint;
        private readonly ReceiptDimensions _receiptDimensions;
        
        private ReceiptPoint(NormalizedPoint normalizedPoint, ReceiptDimensions receiptDimensions)
        {
            _receiptDimensions = receiptDimensions;
            _normalizedPoint = normalizedPoint;
        }

        public static ReceiptPoint FromAbsolute(
            double x, 
            double y, 
            ReceiptDimensions receiptDimensions)
        {
            return new ReceiptPoint(
                new NormalizedPoint(
                    x / receiptDimensions.ImageWidth, 
                    y / receiptDimensions.ImageHeight),
                receiptDimensions);
        }
        
        public static ReceiptPoint FromNormalized(
            double x, 
            double y, 
            ReceiptDimensions receiptDimensions)
        {
            return new ReceiptPoint(
                new NormalizedPoint(x, y),
                receiptDimensions);
        }
    }

    public class AbsolutePoint
    {
        public double X { get; }
        public double Y { get; }

        public AbsolutePoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public class NormalizedPoint
    {
        public double X { get; }
        public double Y { get; }

        public NormalizedPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}