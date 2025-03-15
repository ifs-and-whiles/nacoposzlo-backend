using System;
using System.Collections.Generic;

namespace Billy.MobileApi.Contracts.Expenses
{
    public static partial class Contract
    {
        public static partial class Mobile
        {
            public static partial class V1
            {
                public static class Expenses
                {
                    public class PagedResult<TModel>
                    {
                        public List<TModel> Items { get; set; }
                        public int TotalCount { get; set; }
                    }
                    
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

                    public class GetAllExpensesForUser
                    {
                        public string UserId { get; set; }
                        
                        public int PageNumber { get; set; }
                        public int PageSize { get; set; }
                    }
                    
                    public class GetExpensesRequest
                    {
                        public Guid OwnerId { get; set; }
                    }

                    public class CreateSingleRequest
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

                    public class BulkDeleteRequest
                    {
                        public List<BulkOperationItem<DeleteSingleRequest>> Items { get; set; }
                    }
                    
                    public class BulkCreateRequest
                    {
                        public List<BulkOperationItem<CreateSingleRequest>> Items { get; set; }
                    }

                    public class BulkOperationItem<TEntity>
                    {
                        public string OperationId { get; set; }
                        public TEntity Entity { get; set; }
                    }

                    public class BulkOperationItemResult
                    {
                        public string OperationId { get; set; }
                        public BulkOperationStatus Status { get; set; }
                    }
                    
                    public class BulkOperationItemResult<TResult>
                    {
                        public string OperationId { get; set; }
                        public TResult EntityId { get; set; }
                        public BulkOperationStatus Status { get; set; }
                    }

                    public enum BulkOperationStatus
                    {
                        Ok = 1,
                        UnknownError = 2
                    }
                    
                    public class DeleteSingleRequest
                    {
                        public Guid Id { get; set; }
                    }

                    public class ChangeTagsRequest
                    {
                        public Guid Id { get; set; }
                        public List<string> Tags { get; set; }
                    }
                    public class BulkChangeTagsRequest
                    {
                        public List<BulkOperationItem<ChangeTagsRequest>> Items { get; set; }
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

                    public class BulkChangeDescriptionsRequest
                    {
                        public List<BulkOperationItem<ChangeDescriptionRequest>> Items { get; set; }
                    }
                    
                    public class Seller
                    {
                        public string Name { get; set; }
                        public string PostalCode { get; set; }
                        public string Location { get; set; }
                        public string TaxNumber { get; set; }
                    }
                }
            }
        }
    }
}