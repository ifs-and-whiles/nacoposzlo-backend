using Billy.Domain;

namespace Billy.Receipts.Domain
{
    public class ReceiptDimensions : Value<ReceiptDimensions>
    {
        public int ImageWidth { get; }
                        
        public int ImageHeight { get; }

        public ReceiptDimensions(
            int imageWidth, 
            int imageHeight)
        {
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
        }
        
        public static ReceiptDimensions From(
            int imageWidth,
            int imageHeight) =>
            new ReceiptDimensions(imageWidth, imageHeight);
    }
}