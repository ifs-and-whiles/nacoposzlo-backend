using Billy.CodeReadability;

namespace Billy.Expenses.Domain.Expenses
{
    public class ExpenseUnitPrice : EventSourcing.Value<ExpenseUnitPrice>
    {
        public decimal Value { get; }

        public ExpenseUnitPrice(decimal value)
        {
            Value = value;
        }

        public static Option<ExpenseUnitPrice> From(decimal? unitPrice) => unitPrice.HasValue
            ? new ExpenseUnitPrice(unitPrice.Value)
            : Option<ExpenseUnitPrice>.None;
    }

    public static class ExpenseUnitPriceExtensions
    {
        public static decimal? ValueOrNull(this Option<ExpenseUnitPrice> unitPrice) =>
            unitPrice.Match<decimal?>(value => value.Value, () => null);
    }
}
