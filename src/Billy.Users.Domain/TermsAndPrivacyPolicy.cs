using System;
using Billy.Domain;

namespace Billy.Users.Domain
{
    public class TermsAndPrivacyPolicy : Value<TermsAndPrivacyPolicy>
    {
        public bool WasTermsAndPrivacyPolicyAccepted { get; }
        public DateTimeOffset DateOfConsents { get; }

        public TermsAndPrivacyPolicy(
            bool wasTermsAndPrivacyPolicyAccepted,
            DateTimeOffset dateOfConsents)
        {
            Validate(wasTermsAndPrivacyPolicyAccepted, dateOfConsents);
            
            WasTermsAndPrivacyPolicyAccepted = wasTermsAndPrivacyPolicyAccepted;
            DateOfConsents = dateOfConsents;
        }

        public static TermsAndPrivacyPolicy From(
            bool wasTermsAndPrivacyPolicyAccepted,
            DateTimeOffset dateOfConsents) => 
            new TermsAndPrivacyPolicy(wasTermsAndPrivacyPolicyAccepted, dateOfConsents);

        private void Validate(
            bool wasTermsAndPrivacyPolicyAccepted,
            DateTimeOffset dateOfConsents)
        {
            if (wasTermsAndPrivacyPolicyAccepted == false)
                throw new InvalidValueException(
                    "Acceptance of Terms and Privacy Policy is required",
                    ErrorCodes.AcceptanceOfTermsAndPrivacyPolicyIsRequired);
            
            if (dateOfConsents == DateTimeOffset.MinValue)
                throw new InvalidValueException(
                    "DateOfConsents cannot be min date",
                    ErrorCodes.InvalidDateOfConsents);
        }
    }
}