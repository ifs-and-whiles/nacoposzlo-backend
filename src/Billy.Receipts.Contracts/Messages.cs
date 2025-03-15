using System;
using System.Collections.Generic;
using MassTransit;

namespace Billy.Receipts.Contracts
{
    public static class Messages
    {
        public static class V1
        {
            public class ReceiptRecognizedEvent : CorrelatedBy<Guid>
            {
                public string GlobalUserIdentifier { get; set; }
                public Guid ReceiptId { get; set; }
                public Guid CorrelationId { get; set; }
                
                public RecognizedReceipt RecognizedReceipt { get; set; }
                
                public string AlgorithmName { get; set; }
            }

            public class RecognizedReceipt
            {
                public Orientation OriginalOrientation { get; set; }
                public string Seller { get; set; }
                public string TaxNumber { get; set; }
                public DateTimeOffset? Date { get; set; }
                public decimal? Amount { get; set; }
                public List<Product> Products { get; set; }
                public List<Problem> Problems { get; set; }

                public class Product
                {
                    public bool IsRecognized { get; set; }
                    public bool IsCorrupted { get; set; }
                    public string Name { get; set; }
                    public decimal? Quantity { get; set; }
                    public decimal? UnitPrice { get; set; }
                    public decimal? Amount { get; set; }
                    public BoundingBox BoundingBox { get; set; }
                }

                public enum Problem
                {
                    AmountDifferentThanSumOfProducts = 1
                }

                public class Orientation
                {
                    public double ValueInRadians { get; set; }
                }
            }

            public class ReceiptRecognitionCompletedEvent : CorrelatedBy<Guid>
            {
                public string GlobalUserIdentifier { get; set; }
                public Guid ReceiptId { get; set; }
                public Guid CorrelationId { get; set; }
            }

            public class ReceiptPrintedTextExtractedEvent : CorrelatedBy<Guid>
            {
                public string GlobalUserIdentifier { get; set; }
                public Guid ReceiptId { get; set; }
                public Guid CorrelationId { get; set; }
                public PrintedTextRecognition PrintedTextRecognition { get; set; }
                public int ImageWidth { get; set; }
                public int ImageHeight { get; set; }
            }

            public class PrintedTextRecognition
            {
                public IList<DetectedText> Detections { get; }

                public PrintedTextRecognition(IList<DetectedText> detections)
                {
                    Detections = detections;
                }
                
                public class DetectedText
                {
                    public DetectedText(
                        string text, 
                        NormalizedPoint leftTop, 
                        NormalizedPoint rightTop, 
                        NormalizedPoint leftBottom, 
                        NormalizedPoint rightBottom)
                    {
                        Text = text;
                        LeftTop = leftTop;
                        RightTop = rightTop;
                        LeftBottom = leftBottom;
                        RightBottom = rightBottom;
                    }
                    public string Text { get; }
                    public NormalizedPoint LeftTop { get; }
                    public NormalizedPoint RightTop { get; }
                    public NormalizedPoint LeftBottom { get; }
                    public NormalizedPoint RightBottom { get; }
                }
            }

            public class ReceiptAddedToPrintedTextRecognitionEvent : CorrelatedBy<Guid>
            {
                public string GlobalUserIdentifier { get; set; }
                public Guid ReceiptId { get; set; }
                public Guid CorrelationId { get; set; }
                public int ImageWidth { get; set; }
                public int ImageHeight { get; set; }
            }

            public class BoundingBox
            {
                public NormalizedPoint LeftTop { get; set; }
                public NormalizedPoint RightTop { get; set; }
                public NormalizedPoint LeftBottom { get; set; }
                public NormalizedPoint RightBottom { get; set; }
            }

            public class NormalizedPoint
            {
                public double X { get; set; }
                public double Y { get; set; }
            }
        }
    }
}
