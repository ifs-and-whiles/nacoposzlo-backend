using System;
using Billy.Domain;
using FluentAssertions;
using Xunit;

namespace Billy.Users.Domain.Tests.ValueObjects
{
    public class UserIdTests
    {
        [Fact]
        public void should_fail_validation_for_default_user_id()
        {
            Action action = () => new UserId(default);

            action.Should().Throw<InvalidValueException>().And
                .ErrorCode.Should().Be(ErrorCodes.InvalidUserId);
        }

        [Fact]
        public void should_pass_validation_for_correct_id()
        {
            Action action = () =>new UserId(Guid.NewGuid());
            action.Should().NotThrow();
        }
    }
}