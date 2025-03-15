using System;
using Billy.CodeReadability;
using Billy.Domain;
using Billy.Expenses.Domain.Expenses;

namespace Billy.Expenses.Domain.Shared
{
    public class ReceiptId : EventSourcing.Value<ReceiptId>
    {
        public ReceiptId(Guid value)
        {
            Validate(value);
            Value = value;
        }
        private void Validate(Guid value)
        {
            if(value == Guid.Empty)
                throw new InvalidValueException(
                    "Receipt id can not be default ( empty ) when is specified", 
                    ErrorCodes.InvalidReceiptId);
        }

        public static Option<ReceiptId> From(Guid? value) => value.HasValue
            ? Option<ReceiptId>.Some(new ReceiptId(value.Value))
            : Option<ReceiptId>.None;

        public Guid Value { get; }
        public override string ToString() => Value.ToString();
    }

    public static class ReceiptIdExtensions
    {
        public static Guid? ValueOrNull(this Option<ReceiptId> receiptId) =>
            receiptId.Match<Guid?>(value => value.Value, () => null);
    }
}
