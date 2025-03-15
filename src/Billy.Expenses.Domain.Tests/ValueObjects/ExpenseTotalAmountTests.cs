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
    public class ExpenseTotalAmountTests
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(10)]
        public void should_pass_validation_for_correct_amount(decimal value)
        {
            var totalAmount = new ExpenseTotalAmount(value);
            totalAmount.Value.Should().Be(value);
        }
    }
}
