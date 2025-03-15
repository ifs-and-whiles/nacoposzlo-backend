using System;
using Billy.Domain;
using FluentAssertions;
using Xunit;

namespace Billy.Users.Domain.Tests.ValueObjects
{
    public class TermsAndPrivacyPolicyTests
    {
        [Fact]
        public void should_fail_validation_when_policy_has_not_been_accepted()
        {
            Action action = () => new TermsAndPrivacyPolicy(
                wasTermsAndPrivacyPolicyAccepted: false,
                dateOfConsents: DateTimeOffset.UtcNow);

            action.Should().Throw<InvalidValueException>().And
                .ErrorCode.Should().Be(ErrorCodes.AcceptanceOfTermsAndPrivacyPolicyIsRequired);
        }

        [Fact]
        public void should_fail_validation_when_policy_has_been_accepted_but_is_min_value()
        {
            Action action = () => new TermsAndPrivacyPolicy(
                wasTermsAndPrivacyPolicyAccepted: true,
                dateOfConsents: DateTimeOffset.MinValue);

            action.Should().Throw<InvalidValueException>().And
                .ErrorCode.Should().Be(ErrorCodes.InvalidDateOfConsents);
        }
        
        [Fact]
        public void should_pass_validation_when_policy_has_been_accepted()
        {
            Action action = () => new TermsAndPrivacyPolicy(
                wasTermsAndPrivacyPolicyAccepted: true,
                DateTimeOffset.UtcNow);
            
            action.Should().NotThrow();
        }
    }
}