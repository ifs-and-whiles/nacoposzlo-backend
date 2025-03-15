using System;
using System.Collections.Generic;

namespace Billy.Expenses.Contracts
{
    public static class Commands
    {
        public static class Expenses
        {
            public static class V1
            {
                public class Create
                {
                    public string GlobalUserIdentifier { get; set; }
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

                public class CreateResponse
                {
                    public Guid ExpenseId { get; set; }
                }

                public class Delete
                {
                    public string GlobalUserIdentifier { get; set; }
                    public Guid ExpenseId { get; set; }
                    public override string ToString() => $"{GetType().FullName} {ExpenseId}";

                }

                public class ChangeTags
                {
                    public string GlobalUserIdentifier { get; set; }
                    public Guid ExpenseId { get; set; }
                    public List<string> Tags { get; set; }
                }

                public class ChangeDescription
                {
                    public string GlobalUserIdentifier { get; set; }
                    public Guid ExpenseId { get; set; }
                    public DateTimeOffset Date { get; set; }
                    public Seller Seller { get; set; }
                    public string Title { get; set; }
                    public decimal TotalAmount { get; set; }
                    public decimal? Quantity { get; set; }
                    public decimal? UnitPrice { get; set; }
                    public List<string> Tags { get; set; }
                    public string Comments { get; set; }

                    public override string ToString() => $"{GetType().FullName} {ExpenseId}";

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
