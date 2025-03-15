using System;

namespace Billy.Users.Contracts
{
    public static class Commands
    {
        public static class Users
        {
            public static class V1
            {
                public class Create
                {
                    public string GlobalUserIdentifier { get; set; }
                    public bool WasTermsAndPrivacyPolicyAccepted { get; set; }
                    public DateTimeOffset DateOfConsents { get; set; }
                    
                    public int? ReceiptsRecognitionLimit { get; set; } 
                    public int? ReceiptsRecognitionCurrentPackageCounter { get; set; }
                }

                public class IncreaseReceiptsRecognitionCurrentPackageCounter
                {
                    public string GlobalUserIdentifier { get; set; }
                }

                public class ResetReceiptsRecognitionCurrentPackageCounter
                {
                    public string GlobalUserIdentifier { get; set; }
                }

                public class ResetReceiptsRecognitionCurrentPackageCounterForAllUsers
                {
                    
                }
                
                public class AssignReceiptsRecognitionLimit
                {
                    public string GlobalUserIdentifier { get; set; }
                    public int Limit { get; set; }
                }
            }
        }
        
    }
}