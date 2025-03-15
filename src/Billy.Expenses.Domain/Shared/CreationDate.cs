using System;
using Billy.Domain;
using Billy.Expenses.Domain.Expenses;

namespace Billy.Expenses.Domain.Shared
{
    public class CreationDate : EventSourcing.Value<CreationDate>
    {
        public DateTimeOffset Value { get; }

        public CreationDate(DateTimeOffset value)
        {
            Validate(value);
            Value = value;
        }

        private void Validate(DateTimeOffset value)
        {
            if(!(value.DateTime > DateTime.MinValue && value.DateTime < DateTime.MaxValue))
                throw new InvalidValueException(
                    $"Creation date {value} should be greater than {DateTime.MinValue} and less than {DateTime.MaxValue}",
                    ErrorCodes.InvalidCreationDate);
        }

        public static implicit operator DateTimeOffset(CreationDate date) => date.Value;

        public static CreationDate From(DateTimeOffset value)
        {
            return new CreationDate(value);
        }
    }
}