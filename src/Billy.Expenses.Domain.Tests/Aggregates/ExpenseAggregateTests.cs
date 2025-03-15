using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Billy.Domain;
using Billy.EventSourcing;
using Billy.Expenses.Domain.Expenses;
using Billy.Expenses.Domain.Shared;
using FluentAssertions;
using Xunit;

namespace Billy.Expenses.Domain.Tests.Aggregates
{
    public class ExpenseAggregateTests
    {
        public Fixture Fixture { get; } = new Fixture();

        [Fact]
        public void can_add_new_expense()
        {
            //given
            var id= Fixture.Create<ExpenseId>();
            var userId= Fixture.Create<GlobalUserIdentifier>();
            var date= Fixture.Create<ExpenseDate>();
            var title= Fixture.Create<ExpenseTitle>();
            var totalAmount= Fixture.Create<ExpenseTotalAmount>();
            var seller= Fixture.Create<ExpenseSeller>();
            var tags= Fixture.Create<Tags>();
            var receiptId= Fixture.Create<ReceiptId>();
            var quantity= Fixture.Create<ExpenseQuantity>();
            var unitPrice= Fixture.Create<ExpenseUnitPrice>();
            var comments = Fixture.Create<Comments>();
            var creationDate = Fixture.Create<CreationDate>();

            //when
            var expense = Expense.Create(
                id, userId, date, title,
                totalAmount, seller, tags, creationDate,
                receiptId, quantity, unitPrice,
                comments);

            //then
            expense.GetChanges().Should().BeEquivalentTo(new Events.Expenses.V1.ExpenseAdded(
                expenseId: id.Value,
                globalUserIdentifier: userId.Value,
                date: date.Value,
                title: title.Value,
                receiptId: receiptId.Value,
                seller: new Events.Expenses.V1.Seller(
                    name: seller.Name.ValueOrNull(),
                    postalCode: seller.PostalCode.ValueOrNull(),
                    location: seller.Location.ValueOrNull(),
                    taxNumber: seller.TaxNumber.ValueOrNull()),
                totalAmount: totalAmount.Value,
                quantity: quantity.Value,
                unitPrice: unitPrice.Value,
                tags: tags.ToStringList(),
                comments: comments.Value,
                creationDate: creationDate
            ));
        }

        [Fact]
        public void can_change_description()
        {
            //given
            var expense = new Expense();
            var expenseAdded = Fixture.Create<Events.Expenses.V1.ExpenseAdded>();
            expense.Load(new[] {new StoredEvent(1, "test-stream", expenseAdded, DateTime.UtcNow) });

            //when
            var newDate = Fixture.Create<ExpenseDate>();
            var newTitle = Fixture.Create<ExpenseTitle>();
            var newTotalAmount = Fixture.Create<ExpenseTotalAmount>();
            var newSeller= Fixture.Create<ExpenseSeller>();
            var newQuantity= Fixture.Create<ExpenseQuantity>();
            var newUnitPrice= Fixture.Create<ExpenseUnitPrice>();
            var newComments = Fixture.Create<Comments>();
            var newTags = Fixture.Create<Tags>();

            expense.ChangeDescription(newDate, newTitle,
                newTotalAmount, newSeller, newTags,
                newQuantity, newUnitPrice,
                newComments);

            //then
            expense.GetChanges().Should().BeEquivalentTo(new Events.Expenses.V1.ExpenseDescriptionChanged(
                expenseId: expenseAdded.ExpenseId,
                globalUserIdentifier: expenseAdded.OwnerId,
                date: newDate.Value,
                title: newTitle.Value,
                seller: new Events.Expenses.V1.Seller(
                    name: newSeller.Name.ValueOrNull(),
                    postalCode: newSeller.PostalCode.ValueOrNull(),
                    location: newSeller.Location.ValueOrNull(),
                    taxNumber: newSeller.TaxNumber.ValueOrNull()),
                totalAmount: newTotalAmount.Value,
                quantity: newQuantity.Value,
                unitPrice: newUnitPrice.Value,
                tags: newTags.ToStringList(),
                comments: newComments.Value
                ));
        }

        [Fact]
        public void can_change_tags()
        {
            //given
            var expense = new Expense();
            var expenseAdded = Fixture.Create<Events.Expenses.V1.ExpenseAdded>();
            expense.Load(new[] {new StoredEvent(1, "test-stream", expenseAdded, DateTime.UtcNow) });
            
            //when
            var newTags = Fixture.Create<Tags>();
            
            expense.ChangeTags(newTags);
            
            //then
            expense.GetChanges().Should().BeEquivalentTo(new Events.Expenses.V1.ExpenseTagsChanged(
                expenseId: expenseAdded.ExpenseId,
                globalUserIdentifier: expenseAdded.OwnerId,
                tags: newTags.ToStringList()));
        }

        [Fact]
        public void can_delete_expense()
        {
            //given
            var expense = new Expense();
            var expenseAdded = Fixture.Create<Events.Expenses.V1.ExpenseAdded>();
            expense.Load(new[] {new StoredEvent(1, "test-stream", expenseAdded, DateTime.UtcNow), });

            //when
            expense.Delete();

            //then
            expense.GetChanges().Should().BeEquivalentTo(new Events.Expenses.V1.ExpenseDeleted(
                expenseId: expenseAdded.ExpenseId,
                globalUserIdentifier: expenseAdded.OwnerId));
        }
    }
}
