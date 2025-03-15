namespace Billy.Users.Domain
{
    public static class ErrorCodes
    {
        public const string InvalidUserId = "InvalidUserId";
        public const string InvalidDateOfConsents = "InvalidDateOfConsents";
        public const string AcceptanceOfTermsAndPrivacyPolicyIsRequired = "AcceptanceOfTermsAndPrivacyPolicyIsRequired";
        public const string ReceiptsRecognitionLimitLessThatCurrentCounter = "ReceiptsRecognitionLimitLessThatCurrentCounter";
        public const string UserStreamIdInvalid = "UserStreamIdInvalid";
        public const string ReceiptsRecognitionLimitReached = "ReceiptsRecognitionLimitReached";
        public const string InvalidReceiptsRecognitionLimit = "InvalidReceiptsRecognitionLimit";
        public const string InvalidGlobalUserIdentifier = "InvalidGlobalUserIdentifier";
        public const string InvalidReceiptsRecognitionCurrentPackageCounter = "InvalidReceiptsRecognitionCurrentPackageCounter";
    }
}