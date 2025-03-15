using System;
using System.Collections.Generic;
using System.Dynamic;
using Billy.EventSourcing;

namespace Billy.Expenses.Domain.Expenses
{

    public static class Events
    {
        public static class Expenses
        {
            public static class V1
            {
                [Event]
                public class ExpenseAdded
                {
                    public Guid ExpenseId { get; }
                    public string OwnerId { get; } //GlobalUserIdentifier
                    public DateTimeOffset Date { get; }
                    public DateTimeOffset CreationDate { get; }
                    public Guid? ReceiptId { get; }
                    public Seller Seller { get; }
                    public string Title { get; }
                    public decimal TotalAmount { get; }
                    public decimal? Quantity { get; }
                    public decimal? UnitPrice { get; }
                    public List<string> Tags { get; }
                    public string Comments { get; }

                    public override string ToString()
                        => $"{nameof(ExpenseAdded)}";

                    public ExpenseAdded(
                        Guid expenseId, 
                        string globalUserIdentifier,
                        DateTimeOffset date, 
                        Guid? receiptId, 
                        Seller seller, 
                        string title, 
                        decimal totalAmount, 
                        decimal? quantity, 
                        decimal? unitPrice, 
                        List<string> tags, 
                        string comments, 
                        DateTimeOffset creationDate)
                    {
                        ExpenseId = expenseId;
                        OwnerId = globalUserIdentifier;
                        Date = date;
                        ReceiptId = receiptId;
                        Seller = seller;
                        Title = title;
                        TotalAmount = totalAmount;
                        Quantity = quantity;
                        UnitPrice = unitPrice;
                        Tags = tags ?? new List<string>();
                        Comments = comments;
                        CreationDate = creationDate;
                    }
                }

                [Event]
                public class ExpenseDescriptionChanged
                {
                    public ExpenseDescriptionChanged(
                        Guid expenseId, 
                        string globalUserIdentifier, 
                        DateTimeOffset date, 
                        Seller seller, 
                        string title, 
                        decimal totalAmount, 
                        decimal? quantity, 
                        decimal? unitPrice, 
                        List<string> tags, 
                        string comments)
                    {
                        ExpenseId = expenseId;
                        OwnerId = globalUserIdentifier;
                        Date = date;
                        Seller = seller;
                        Title = title;
                        TotalAmount = totalAmount;
                        Quantity = quantity;
                        UnitPrice = unitPrice;
                        Tags = tags;
                        Comments = comments;
                    }

                    public Guid ExpenseId { get; }
                    public string OwnerId { get; } //GlobalUserIdentifier
                    public DateTimeOffset Date { get; }
                    public Seller Seller { get; }
                    public string Title { get; }
                    public decimal TotalAmount { get; }
                    public decimal? Quantity { get; }
                    public decimal? UnitPrice { get; }
                    public List<string> Tags { get; }
                    public string Comments { get; }

                    public override string ToString()
                        => $"{nameof(ExpenseDescriptionChanged)}";
                }


                [Event]
                public class ExpenseDeleted
                {
                    public ExpenseDeleted(
                        Guid expenseId, 
                        string globalUserIdentifier)
                    {
                        OwnerId = globalUserIdentifier;
                        ExpenseId = expenseId;
                    }

                    public Guid ExpenseId { get; }
                    public string OwnerId { get; } //GlobalUserIdentifier

                    public override string ToString()
                        => $"{nameof(ExpenseDeleted)}";
                }

                [Event]
                public class ExpenseTagsChanged
                {
                    public ExpenseTagsChanged(
                        Guid expenseId, 
                        string globalUserIdentifier, 
                        List<string> tags)
                    {
                        OwnerId = globalUserIdentifier;
                        Tags = tags;
                        ExpenseId = expenseId;
                    }

                    public Guid ExpenseId { get; }
                    public string OwnerId { get; } //GlobalUserIdentifier
                    
                    public List<string> Tags { get; }

                    public override string ToString()
                        => $"{nameof(ExpenseTagsChanged)}";
                }

                public class Seller
                {
                    public Seller(
                        string name, 
                        string postalCode, 
                        string location, 
                        string taxNumber)
                    {
                        Name = name;
                        PostalCode = postalCode;
                        Location = location;
                        TaxNumber = taxNumber;
                    }

                    public string Name { get; }
                    public string PostalCode { get; }
                    public string Location { get; }
                    public string TaxNumber { get; }
                }

            }
        }
 
    }
}
