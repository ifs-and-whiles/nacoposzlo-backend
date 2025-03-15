using System;
using System.Collections.Generic;
using System.Text;

namespace Billy.Receipts.Domain
{
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
            public double ValueInRadians { get; }

            public Orientation(double valueInRadians)
            {
                ValueInRadians = valueInRadians;
            }
        }
    }
}
