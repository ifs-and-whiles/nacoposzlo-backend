using System;
using System.Linq;
using System.Threading.Tasks;
using Billy.CodeReadability;
using Billy.Domain;
using Billy.EventSourcing;
using Billy.Expenses.Domain.Shared;

namespace Billy.Expenses.Domain.Expenses
{
    public class Expense : AggregateRoot<ExpenseStreamId>
    {
        public GlobalUserIdentifier GlobalUserIdentifier { get; private set; }
        public ExpenseId ExpenseId { get; private set; }
        
        public static Expense Create(
            ExpenseId expenseId,
            GlobalUserIdentifier globalUserIdentifier,
            ExpenseDate date,
            ExpenseTitle title,
            ExpenseTotalAmount totalAmount,
            ExpenseSeller seller,
            Tags tags,
            CreationDate creationDate,

            Option<ReceiptId> receiptId,
            Option<ExpenseQuantity> quantity,
            Option<ExpenseUnitPrice> unitPrice,
            Option<Comments> comments)
        {

            var expense = new Expense();

            expense.Apply(new Events.Expenses.V1.ExpenseAdded(
                expenseId: expenseId,
                globalUserIdentifier: globalUserIdentifier,
                date: date,
                title: title,
                totalAmount: totalAmount,
                receiptId: receiptId.ValueOrNull(),
                seller: new Events.Expenses.V1.Seller(
                    seller.Name.ValueOrNull(),
                    seller.PostalCode.ValueOrNull(),
                    seller.Location.ValueOrNull(),
                    seller.TaxNumber.ValueOrNull()),
                quantity: quantity.ValueOrNull(),
                unitPrice: unitPrice.ValueOrNull(),
                tags: tags.ToStringList(),
                comments: comments.ValueOrNull(),
                creationDate: creationDate));

            return expense;
        }

        public void ChangeDescription(
            ExpenseDate date,
            ExpenseTitle title,
            ExpenseTotalAmount totalAmount,
            ExpenseSeller seller,
            Tags tags,

            Option<ExpenseQuantity> quantity,
            Option<ExpenseUnitPrice> unitPrice,
            Option<Comments> comments)
        {

            Apply(@event: new Events.Expenses.V1.ExpenseDescriptionChanged(
                expenseId: ExpenseId,
                globalUserIdentifier: GlobalUserIdentifier,
                date: date,
                title: title,
                totalAmount: totalAmount,
                seller: new Events.Expenses.V1.Seller(
                    seller.Name.ValueOrNull(),
                    seller.PostalCode.ValueOrNull(),
                    seller.Location.ValueOrNull(),
                    seller.TaxNumber.ValueOrNull()),
                quantity: quantity.ValueOrNull(),
                unitPrice: unitPrice.ValueOrNull(),
                tags: tags.ToStringList(),
                comments: comments.ValueOrNull()));
        }

        public void Delete()
            => Apply(new Events.Expenses.V1.ExpenseDeleted(ExpenseId, GlobalUserIdentifier));

        public void ChangeTags(Tags tags)
        {
            Apply(@event: new Events.Expenses.V1.ExpenseTagsChanged(
                expenseId: ExpenseId,
                globalUserIdentifier: GlobalUserIdentifier,
                tags: tags.ToStringList()));
        }

        protected override void When(object @event)
        {
            switch (@event)
            {
                case Events.Expenses.V1.ExpenseAdded e:
                    Id = ExpenseStreamId.From(ExpenseId.From(e.ExpenseId));
                    ExpenseId = ExpenseId.From(e.ExpenseId);
                    GlobalUserIdentifier = GlobalUserIdentifier.From(e.OwnerId);
                    break;
            }
        }

        protected override void EnsureValidState()
        {

        }
    }
}
