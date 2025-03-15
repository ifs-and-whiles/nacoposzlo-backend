using Billy.CodeReadability;
using Billy.Domain;

namespace Billy.Users.Domain
{
    public class ReceiptsRecognitionLimit : Value<ReceiptsRecognitionLimit>
    {
        private static int _defaultLimit = 100;
        protected ReceiptsRecognitionLimit(int value)
        {
            Validate(value);
            Value = value;
            IsUnlimited = Value == -1;
        }

        private void Validate(int? value)
        {
            if (value != null && (value < 0 && value != -1))
                throw new InvalidValueException(
                    "ReceiptsRecognitionLimit can not be negative",
                    ErrorCodes.InvalidReceiptsRecognitionLimit);
        }

        public static ReceiptsRecognitionLimit From(int? value) => value.HasValue
        ? new ReceiptsRecognitionLimit(value.Value)
        : new ReceiptsRecognitionLimit(_defaultLimit);
        
        public static ReceiptsRecognitionLimit Default => new ReceiptsRecognitionLimit(_defaultLimit);
        
        public int Value { get; }
        public bool IsUnlimited { get; }

        public static implicit operator int(ReceiptsRecognitionLimit self) => self.Value;

        public override string ToString() => Value.ToString(); 
    }
}