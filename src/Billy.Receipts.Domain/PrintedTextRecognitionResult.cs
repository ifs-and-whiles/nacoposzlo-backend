using System.Collections.Generic;
using System.Linq;

namespace Billy.Receipts.Domain
{
    public class IReceipt
    {
        public PrintedTextRecognitionResult RecognitionResult { get; }
        public ReceiptDimensions Dimensions { get; }
    }
    

    public class PrintedTextRecognitionResult
    {
        public IReadOnlyList<DetectedText> Detections { get; }

        public PrintedTextRecognitionResult(
            IEnumerable<DetectedText> detections)
        {
            Detections = detections.ToArray();
        }
    }

    public class DetectedText
    {
        public DetectedText(
            string text, 
            ReceiptPoint leftTop, 
            ReceiptPoint rightTop, 
            ReceiptPoint leftBottom, 
            ReceiptPoint rightBottom)
        {
            Text = text;
            LeftTop = leftTop;
            RightTop = rightTop;
            LeftBottom = leftBottom;
            RightBottom = rightBottom;
        }
        public string Text { get; }
        public ReceiptPoint LeftTop { get; }
        public ReceiptPoint RightTop { get; }
        public ReceiptPoint LeftBottom { get; }
        public ReceiptPoint RightBottom { get; }
    }
}
