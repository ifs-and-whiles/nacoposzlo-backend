namespace Billy.Receipts.Domain
{
    public class BoundingBox
    {
        public ReceiptPoint LeftTop { get; set; }
        public ReceiptPoint RightTop { get; set; }
        public ReceiptPoint LeftBottom { get; set; }
        public ReceiptPoint RightBottom { get; set; }
    }
}