using System;
using Billy.Domain;

namespace Billy.Users.Domain
{
    public class UserId : Value<UserId>
    {
        public UserId(Guid value)
        {
            Validate(value);
            Value = value;
        }

        private void Validate(Guid value)
        {
            if (value == Guid.Empty)
                throw new InvalidValueException(
                    "User id can not be empty",
                    ErrorCodes.InvalidUserId);
        }

        public static UserId From(Guid value) => new UserId(value);

        public Guid Value { get; }

        public static implicit operator Guid(UserId self) => self.Value;

        public override string ToString() => Value.ToString();        
    }
}