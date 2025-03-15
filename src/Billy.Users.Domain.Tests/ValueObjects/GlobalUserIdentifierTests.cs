using System;
using Billy.Domain;
using FluentAssertions;
using Xunit;

namespace Billy.Users.Domain.Tests.ValueObjects
{
    public class GlobalUserIdentifierTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void should_fail_validation_for_empty_global_user_identifier(string userIdentifier)
        {
            Action action = () => new GlobalUserIdentifier(userIdentifier);

            action.Should().Throw<InvalidValueException>().And
                .ErrorCode.Should().Be(ErrorCodes.InvalidGlobalUserIdentifier);
        }

        [Fact]
        public void should_pass_validation_for_correct_user_identifier()
        {
            Action action = () =>new GlobalUserIdentifier(Guid.NewGuid().ToString());
            action.Should().NotThrow();
        }
    }
}