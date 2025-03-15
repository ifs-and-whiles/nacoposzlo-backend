using System;
using System.Collections.Generic;
using System.Text;
using Billy.Domain;
using Billy.EventSourcing;
using Billy.Expenses.Domain.Expenses;
using FluentAssertions;
using Xunit;

namespace Billy.Expenses.Domain.Tests.ValueObjects
{
    public class ExpenseDateTests
    {
        [Fact]
        public void should_fail_validation_when_date_is_equal_date_max()
        {
            var exception = Assert.Throws<InvalidValueException>(() => new ExpenseDate(DateTimeOffset.MaxValue));
            exception.ErrorCode.Should().Be(ErrorCodes.InvalidExpenseDate);
        }

        [Fact]
        public void should_fail_validation_when_date_is_equal_date_min()
        {
            var exception = Assert.Throws<InvalidValueException>(() => new ExpenseDate(DateTimeOffset.MinValue));
            exception.ErrorCode.Should().Be(ErrorCodes.InvalidExpenseDate);
        }

        [Fact]
        public void should_pass_validation_for_correct_date()
        {
            var validateAction = new Action(() => new ExpenseDate(new DateTimeOffset(2019,01,01, 10,10,10, TimeSpan.Zero)));
            validateAction.Should().NotThrow();
        }
    }
}
