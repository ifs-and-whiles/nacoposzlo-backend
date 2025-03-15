using System;
using Billy.EventSourcing;

namespace Billy.Users.Domain
{
    public static class Events
    {
        public static class Users
        {
            public static class V1
            {
                [Event]
                public class UserAdded
                {
                    public UserAdded(
                        Guid id, 
                        string globalUserIdentifier, 
                        int receiptsRecognitionLimit, 
                        int receiptsRecognitionCurrentPackageCounter,
                        TermsAndPrivacyPolicy termsAndPrivacyPolicy)
                    {
                        Id = id;
                        GlobalUserIdentifier = globalUserIdentifier;
                        ReceiptsRecognitionLimit = receiptsRecognitionLimit;
                        ReceiptsRecognitionCurrentPackageCounter = receiptsRecognitionCurrentPackageCounter;
                        TermsAndPrivacyPolicy = termsAndPrivacyPolicy;
                    }
                    public Guid Id { get; }
                    public string GlobalUserIdentifier { get; }
                    public int ReceiptsRecognitionLimit { get; }
                    public int ReceiptsRecognitionCurrentPackageCounter { get; }
                    public TermsAndPrivacyPolicy TermsAndPrivacyPolicy { get; }
                }

                public class TermsAndPrivacyPolicy
                {
                    public bool WasTermsAndPrivacyPolicyAccepted { get; set; }
                    public DateTimeOffset DateOfConsents { get; set; }
                }

                [Event]
                public class ReceiptsRecognitionLimitAssigned
                {
                    public ReceiptsRecognitionLimitAssigned(Guid id, int limit)
                    {
                        Id = id;
                        Limit = limit;
                    }
                    public Guid Id { get;  }
                    public int Limit { get; }
                }

                [Event]
                public class ReceiptsRecognitionCurrentPackageCounterIncreased
                {
                    public ReceiptsRecognitionCurrentPackageCounterIncreased(Guid id)
                    {
                        Id = id;
                    }
                    public Guid Id { get; }
                }

                [Event]
                public class ReceiptsRecognitionLimitReached
                {
                    public ReceiptsRecognitionLimitReached(Guid id)
                    {
                        Id = id;
                    }
                    public Guid Id { get; }
                }

                [Event]
                public class ReceiptsRecognitionCounterZeroed
                {
                    public ReceiptsRecognitionCounterZeroed(Guid id)
                    {
                        Id = id;
                    }
                    public Guid Id { get; }
                }
            }
        }
    }
}