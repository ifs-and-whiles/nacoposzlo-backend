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
    public class ExpenseTitleTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void should_fail_validation_for_empty_or_whitespace_title(string title)
        {
            var exception = Assert.Throws<InvalidValueException>(() => new ExpenseTitle(title));
            exception.ErrorCode.Should().Be(ErrorCodes.InvalidExpenseTitle);
        }

        [Fact]
        public void should_pass_validation_for_correct_title()
        {
            new ExpenseTitle("correct title");
        }
    }
}
