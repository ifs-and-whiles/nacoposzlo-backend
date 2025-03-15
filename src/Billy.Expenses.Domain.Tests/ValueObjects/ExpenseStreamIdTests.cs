using System;
using Billy.Domain;
using Billy.Expenses.Domain.Expenses;
using FluentAssertions;
using Xunit;

namespace Billy.Expenses.Domain.Tests.ValueObjects
{
    public class ExpenseStreamIdTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("testststsst")]
        public void should_fail_validation_for_wrong_expense_stream_id(string expenseStreamId)
        {
            Action action = () => new ExpenseStreamId(expenseStreamId);

            action.Should().Throw<InvalidValueException>().And
                .ErrorCode.Should().Be(ErrorCodes.ExpenseStreamIdInvalid);
        }

        [Fact]
        public void should_pass_validation_for_correct_expense_stream_id()
        {
            Action action = () =>new ExpenseStreamId($"Expense-{Guid.NewGuid().ToString()}");
            action.Should().NotThrow();
        }
    }
}