using System;
using Billy.Domain;
using Billy.Expenses.Domain.Expenses;

namespace Billy.Expenses.Domain.Shared
{
    public class GlobalUserIdentifier : EventSourcing.Value<GlobalUserIdentifier>
    {
        protected GlobalUserIdentifier(string value)
        {
            Validate(value);
            Value = value;
        }

        private void Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidValueException(
                    "User id can not be empty",
                    ErrorCodes.InvalidUserId);
        }

        public static GlobalUserIdentifier From(string value) => new GlobalUserIdentifier(value);

        public string Value { get; }

        public static implicit operator string(GlobalUserIdentifier self) => self.Value;

        public override string ToString() => Value;
    }
}
