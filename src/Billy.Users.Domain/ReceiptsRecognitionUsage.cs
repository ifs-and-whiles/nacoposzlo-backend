using Billy.Domain;

namespace Billy.Users.Domain
{
    public class ReceiptsRecognitionUsage : Value<ReceiptsRecognitionUsage>
    {
        public ReceiptsRecognitionLimit Limit { get; private set;}
        public ReceiptsRecognitionCurrentPackageCounter CurrentPackageCounter { get; private set; }

        public bool LimitReached => Limit == CurrentPackageCounter && !Limit.IsUnlimited;

        protected ReceiptsRecognitionUsage(
            ReceiptsRecognitionLimit limit, 
            ReceiptsRecognitionCurrentPackageCounter counter)
        {
            Validate(limit, counter);
            Limit = limit;
            CurrentPackageCounter = counter;
        }

        private void Validate(
            ReceiptsRecognitionLimit limit, 
            ReceiptsRecognitionCurrentPackageCounter counter)
        {
            ValidateLimit(limit, counter);
        }

        private void ValidateLimit(ReceiptsRecognitionLimit limit, ReceiptsRecognitionCurrentPackageCounter counter)
        {
            if (limit < counter && !limit.IsUnlimited)
                throw new InvalidValueException(
                    "ReceiptsRecognitionLimit can not be less than ReceiptsRecognitionCurrentPackageCounter",
                    ErrorCodes.ReceiptsRecognitionLimitLessThatCurrentCounter);
        }

        public void IncreaseCounter() => 
            CurrentPackageCounter++;

        public void ResetCounter() =>
            CurrentPackageCounter = ReceiptsRecognitionCurrentPackageCounter.From(0);
        
        public static ReceiptsRecognitionUsage From(
            ReceiptsRecognitionLimit limit, 
            ReceiptsRecognitionCurrentPackageCounter counter) => new ReceiptsRecognitionUsage(limit, counter);

        public void AssignLimit(int limit)
        {
            var newLimit = ReceiptsRecognitionLimit.From(limit);
            
            ValidateLimit(newLimit, CurrentPackageCounter);
            
            Limit = newLimit;
        }
            
    }
}