using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Billy.MobileApi.Contracts.Receipts
{
    public static partial class Contract
    {
        public static partial class Mobile
        {
            public static partial class V1
            {
                public static class Receipts
                {
                    public class GetReceiptImageRequest
                    {
                        public Guid ReceiptId { get; set; }
                    }
                    
                    public class GetRecognizedReceiptRequest
                    {
                        public Guid ReceiptId { get; set; }
                    }

                    public class GetReceiptRecognitionProcessStatusRequest
                    {
                        public Guid ReceiptId { get; set; }
                    }

                    public class RecognizeReceiptRequest
                    {
                        public IFormFile Image { get; set; }
                        
                        public int? ImageWidth { get; set; }
                        
                        public int? ImageHeight { get; set; }
                    }

                    public class ReceiptRecognitionProcessState
                    {
                        public ReceiptRecognitionProcessStatus Status { get; set; }
                    }

                    public enum ReceiptRecognitionProcessStatus
                    {
                        RecognitionStarted = 1,
                        RecognitionFailed = 2,
                        RecognitionCompleted = 3
                    }

                    public class Receipt
                    {
                        public Guid Id { get; set; }
                        public Orientation OriginalOrientation { get; set; }
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