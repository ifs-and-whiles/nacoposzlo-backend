﻿using System;
using Billy.Domain;
using Billy.Expenses.Domain.Expenses;
using Billy.Expenses.Domain.Shared;
using FluentAssertions;
using Xunit;

namespace Billy.Expenses.Domain.Tests.ValueObjects
{
    public class CreationDateTests
    {
        [Fact]
        public void should_fail_validation_when_date_is_equal_date_max()
        {
            var exception = Assert.Throws<InvalidValueException>(() => new CreationDate(DateTimeOffset.MaxValue));
            exception.ErrorCode.Should().Be(ErrorCodes.InvalidCreationDate);
        }

        [Fact]
        public void should_fail_validation_when_date_is_equal_date_min()
        {
            var exception = Assert.Throws<InvalidValueException>(() => new CreationDate(DateTimeOffset.MinValue));
            exception.ErrorCode.Should().Be(ErrorCodes.InvalidCreationDate);
        }

        [Fact]
        public void should_pass_validation_for_correct_date()
        {
            var validateAction = new Action(() => new CreationDate(new DateTimeOffset(2019,01,01, 10,10,10, TimeSpan.Zero)));
            validateAction.Should().NotThrow();
        }
    }
}