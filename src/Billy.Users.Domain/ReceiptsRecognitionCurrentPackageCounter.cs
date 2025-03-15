using Billy.Domain;

namespace Billy.Users.Domain
{
    public class ReceiptsRecognitionCurrentPackageCounter : Value<ReceiptsRecognitionCurrentPackageCounter>
    {
        private static int _defaultCounter = 0;
        protected ReceiptsRecognitionCurrentPackageCounter(int value)
        {
            Validate(value);
            Value = value;
        }

        private void Validate(int? value)
        {
            if (value != null && value < 0)
                throw new InvalidValueException(
                    "ReceiptsRecognitionCurrentPackageCounter can not be negative",
                    ErrorCodes.InvalidReceiptsRecognitionCurrentPackageCounter);
        }

        public static ReceiptsRecognitionCurrentPackageCounter operator ++ (ReceiptsRecognitionCurrentPackageCounter counter)
        {
            return new ReceiptsRecognitionCurrentPackageCounter(counter.Value + 1);
        }
        
        public static ReceiptsRecognitionCurrentPackageCounter From(int? value) => value.HasValue
        ? new ReceiptsRecognitionCurrentPackageCounter(value.Value)
        : new ReceiptsRecognitionCurrentPackageCounter(_defaultCounter);
        
        public static ReceiptsRecognitionCurrentPackageCounter Default => new ReceiptsRecognitionCurrentPackageCounter(_defaultCounter);
        

        public int Value { get; }

        public static implicit operator int(ReceiptsRecognitionCurrentPackageCounter self) => self.Value;

        public override string ToString() => Value.ToString(); 
    }
}