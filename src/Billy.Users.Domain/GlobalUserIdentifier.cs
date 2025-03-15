using System;
using Billy.Domain;
using Billy.EventSourcing;

namespace Billy.Users.Domain
{
    public class GlobalUserIdentifier : Billy.Domain.Value<GlobalUserIdentifier>
    {
        public GlobalUserIdentifier(string value)
        {
            Validate(value);
            Value = value;
        }

        private void Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidValueException(
                    "GlobalUserIdentifier can not be empty",
                    ErrorCodes.InvalidGlobalUserIdentifier);
        }

        public static GlobalUserIdentifier From(string value) => new GlobalUserIdentifier(value);

        public string Value { get; }

        public static implicit operator string(GlobalUserIdentifier self) => self.Value;

        public override string ToString() => Value.ToString();    
    }
    
}