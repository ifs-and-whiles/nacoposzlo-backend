using System;

namespace Billy.MobileApi.Contracts.Users
{
    public static partial class Contract
    {
        public static partial class Mobile
        {
            public static partial class V1
            {
                public static class Users
                {
                    public class RegisterMeRequest
                    {
                        public bool WasTermsAndPrivacyPolicyAccepted { get; set; }
                        public DateTimeOffset DateOfConsents { get; set; }
                    }
                    
                    public class User
                    {
                        public ReceiptsRecognitionUsage ReceiptsRecognitionUsage { get; set; }
                        public bool DisplayAds { get; set; }
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