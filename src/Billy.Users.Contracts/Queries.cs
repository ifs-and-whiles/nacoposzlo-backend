using System;

namespace Billy.Users.Contracts
{
    public static class Queries
    {
        public static class Users
        {
            public static class V1
            {
                public class GetUser
                {
                    public string GlobalUserIdentifier { get; set; }
                }

                public class GetUserReceiptsRecognitionUsageStatus
                {
                    public string GlobalUserIdentifier { get; set; }
                }
                
                public static class ReadModels
                {
                    public class User
                    {
                        public Guid Id { get; set; }
                        public string GlobalUserIdentifier { get; set; }
                        public ReceiptsRecognitionUsage ReceiptsRecognitionUsage { get; set; }
                        public bool DisplayAds { get; set; }
                        
                        public TermsAndPrivacyPolicy TermsAndPrivacyPolicy { get; set; }
                    }
    
                    public class TermsAndPrivacyPolicy
                    {
                        public bool WasTermsAndPrivacyPolicyAccepted { get; set; }
                        public DateTimeOffset DateOfConsents { get; set; }
                    }
                    
                    public class ReceiptsRecognitionUsage
                    {
                        public int TotalUtilizationCounter { get; set; }
                        public int Limit { get; set; }
                        public int CurrentPackageCounter { get; set; }
                        public bool LimitExceeded { get; set; }
                    }
                }
            }
        }
    }
}