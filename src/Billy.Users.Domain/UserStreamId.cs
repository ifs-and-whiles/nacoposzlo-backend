using Billy.Domain;
using Billy.EventSourcing;

namespace Billy.Users.Domain
{
    public class UserStreamId: Billy.Domain.Value<UserStreamId>
    {
        public string Value { get; }

        public static string StreamPrefix { get; } = "User-";

        public static UserStreamId From(GlobalUserIdentifier userIdentifier) =>
            new UserStreamId($"{StreamPrefix}{userIdentifier}");

        public UserStreamId(string value)
        {
            if (string.IsNullOrEmpty(value) || !value.StartsWith(StreamPrefix))
                throw new InvalidValueException($"Invalid value {value} for user stream id, it has to start with '{StreamPrefix}' prefix", ErrorCodes.UserStreamIdInvalid);

            Value = value;
        }

        public static implicit operator string(UserStreamId self) => self.Value;
    }
}