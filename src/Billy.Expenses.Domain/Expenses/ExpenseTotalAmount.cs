using Billy.Domain;
using ValueObjectValidation = Billy.EventSourcing.ValueObjectValidation;

namespace Billy.Expenses.Domain.Expenses
{
    public class ExpenseTotalAmount : EventSourcing.Value<ExpenseTotalAmount>
    {
        public decimal Value { get; }

        public ExpenseTotalAmount(decimal value)
        {
            Value = value;
        }

        public static implicit operator decimal(ExpenseTotalAmount totalAmount) => totalAmount.Value;

        public static ExpenseTotalAmount From(decimal amount) => new ExpenseTotalAmount(amount);
    }
}
