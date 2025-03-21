﻿using Billy.CodeReadability;
using Billy.Domain;

namespace Billy.Expenses.Domain.Expenses
{
    public class ExpenseQuantity : EventSourcing.Value<ExpenseQuantity>
    {
        public decimal Value { get; }

        public ExpenseQuantity(decimal value)
        {
            Value = value;
        }

        public static Option<ExpenseQuantity> From(decimal? quantity) => quantity.HasValue
            ? new ExpenseQuantity(quantity.Value)
            : Option<ExpenseQuantity>.None;
    }

    public static class ExpenseQuantityExtensions
    {
        public static decimal? ValueOrNull(this Option<ExpenseQuantity> quantity) =>
            quantity.Match<decimal?>(value => value.Value, () => null);
    }
}
