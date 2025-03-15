using System;
using System.Collections.Generic;

namespace Billy.Expenses.Contracts
{
    public static class Queries
    {
        public static class Expenses
        {
            public static class V1
            {
                public class PagedResult<TModel>
                {
                    public List<TModel> Items { get; set; }
                    public int TotalCount { get; set; }
                }
                
                public class GetExpense
                {
                    public Guid ExpenseId { get; set; }
                    
                    public string GlobalUserIdentifier { get; set; }
                }

                public class GetExpenses
                {
                    public string GlobalUserIdentifier { get; set; }
                    public int PageSize { get; set; } = 100;
                    public int PageNumber { get; set; } = 1;
                }

                public static class ReadModels
                {
                    public class Expense
                    {
                        public Guid Id { get; set; } //ExpenseId
                        public string OwnerId { get; set; } //GlobalUserIdentifier
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
