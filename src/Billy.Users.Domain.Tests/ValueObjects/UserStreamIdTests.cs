using System;
using Billy.Domain;
using FluentAssertions;
using Xunit;

namespace Billy.Users.Domain.Tests.ValueObjects
{
    public class UserStreamIdTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("testststsst")]
        public void should_fail_validation_for_wrong_user_stream_id(string userStreamId)
        {
            Action action = () => new UserStreamId(userStreamId);

            action.Should().Throw<InvalidValueException>().And
                .ErrorCode.Should().Be(ErrorCodes.UserStreamIdInvalid);
        }

        [Fact]
        public void should_pass_validation_for_correct_user_stream_id()
        {
            Action action = () =>new UserStreamId($"User-{Guid.NewGuid().ToString()}");
            action.Should().NotThrow();
        }
    }
}