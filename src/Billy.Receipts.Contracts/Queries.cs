using System;
using System.Collections.Generic;
using System.IO;

namespace Billy.Receipts.Contracts
{
    public static class Queries
    {
        public static class Receipts
        {
            public static class V1
            {
                public class GetRecognizedReceipt
                {
                    public Guid ReceiptId { get; set; }
                }

                public class GetReceiptRecognitionProcessStatus
                {
                    public Guid ReceiptId { get; set; }
                }

                public class GetReceiptImage
                {
                    public Guid ReceiptId { get; set; }
                }

                public static class ReadModels
                {
                    public class ReceiptImage
                    {
                        public Stream Image { get; set; }
                        public string ContentType { get; set; }
                    }

                    public class ReceiptRecognitionProcessState
                    {
                        public Guid Id { get; set; } //ReceiptId
                        public string UserId { get; set; } //GlobalUserIdentifier
                        public ReceiptRecognitionProcessStatus Status { get; set; }
                        public DateTimeOffset OperationStartDate { get; set; }
                        public DateTimeOffset? OperationEndDate { get; set; }
                    }

                    public enum ReceiptRecognitionProcessStatus
                    {
                        RecognitionStarted = 1,
                        RecognitionFailed = 2,
                        RecognitionCompleted = 3
                    }

                    public class Receipt
                    {
                        public Orientation OriginalOrientation { get; set; }
                        public Guid Id { get; set; } //ReceiptId
                        public string UserId { get; set; } //GlobalUserIdentifier
                        public string Seller { get; set; }
                        public string TaxNumber { get; set; }
                        public DateTimeOffset? Date { get; set; }
                        public decimal? Amount { get; set; }
                        public List<Product> Products { get; set; }
                        public List<Problem> Problems { get; set; }
                    }

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

                    public class BoundingBox
                    {
                        public Point LeftTop { get; set; }
                        public Point RightTop { get; set; }
                        public Point LeftBottom { get; set; }
                        public Point RightBottom { get; set; }
                    }

                    public class Point
                    {
                        public double X { get; set; }
                        public double Y { get; set; }
                    }

                    public class Orientation
                    {
                        public double ValueInRadians { get; set; }
                    }
                }
            }
        }
    }
}
