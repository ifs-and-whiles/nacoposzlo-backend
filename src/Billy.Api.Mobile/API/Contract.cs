using System;
using System.Collections.Generic;
using System.Text;

namespace Billy.Api.Mobile.API
{
    public static class Contract
    {
        public static class Mobile
        {
            public static class V1
            {
                public static class Expenses
                {
                    public class Expense
                    {
                        public Guid Id { get; set; }
                        public DateTimeOffset Date { get; set; }
                        public Guid? ReceiptId { get; set; }
                        public string Title { get; set; }
                        public decimal TotalAmount { get; set; }
                        public decimal? Quantity { get; set; }
                        public decimal? UnitPrice { get; set; }
                        public List<string> Tags { get; set; }
                        public Seller Seller { get; set; }
                        public string Comments { get; set; }
                    }

                    public class GetSingleExpenseRequest
                    {
                        public Guid Id { get; set; }
                    }

                    public class GetExpensesRequest
                    {
                        public Guid OwnerId { get; set; }
                    }

                    public class CreateRequest
                    {
                        public Guid OwnerId { get; set; }
                        public DateTimeOffset Date { get; set; }
                        public Guid? ReceiptId { get; set; }
                        public Seller Seller { get; set; }
                        public string Title { get; set; }
                        public decimal TotalAmount { get; set; }
                        public decimal? Quantity { get; set; }
                        public decimal? UnitPrice { get; set; }
                        public List<string> Tags { get; set; }
                        public string Comments { get; set; }
                    }

                    public class DeleteRequest
                    {
                        public Guid Id { get; set; }
                    }

                    public class AssignTagsRequest
                    {
                        public Guid ExpenseId { get; set; }
                        public List<string> Tags { get; set; }
                    }

                    public class UnassignTagsRequest
                    {
                        public Guid ExpenseId { get; set; }
                        public List<string> Tags { get; set; }
                    }

                    public class ChangeTotalAmountRequest
                    {
                        public Guid Id { get; set; }
                        public decimal TotalAmount { get; set; }
                    }

                    public class ChangeDateRequest
                    {
                        public Guid Id { get; set; }
                        public DateTimeOffset Date { get; set; }
                    }

                    public class ChangeDescriptionRequest
                    {
                        public Guid Id { get; set; }
                        public DateTimeOffset Date { get; set; }
                        public Seller Seller { get; set; }
                        public string Title { get; set; }
                        public decimal TotalAmount { get; set; }
                        public decimal? Quantity { get; set; }
                        public decimal? UnitPrice { get; set; }
                        public List<string> Tags { get; set; }
                        public string Comments { get; set; }
                    }

                    public class Seller
                    {
                        public string Name { get; set; }
                        public string PostalCode { get; set; }
                        public string Location { get; set; }
                        public string TaxNumber { get; set; }
                    }
                }

                public static class Receipts
                {
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
                        public string ImageInBase64 { get; set; }
                        public string ImageExtension { get; set; }
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
                    }

                    public enum Problem
                    {
                        AmountDifferentThanSumOfProducts = 1
                    }
                }
            }
        }
    }
}
