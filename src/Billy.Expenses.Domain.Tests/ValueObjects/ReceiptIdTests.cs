using System;
using System.Collections.Generic;
using System.Text;
using Billy.Domain;
using Billy.EventSourcing;
using Billy.Expenses.Domain.Expenses;
using Billy.Expenses.Domain.Shared;
using FluentAssertions;
using Xunit;

namespace Billy.Expenses.Domain.Tests.ValueObjects
{
    public class ReceiptIdTests
    {
        [Fact]
        public void should_fail_validation_for_default_receipt_id()
        {
            Action action = () => new ReceiptId(default);

            action.Should().Throw<InvalidValueException>().And
                .ErrorCode.Should().Be(ErrorCodes.InvalidReceiptId);
        }

        [Fact]
        public void should_pass_validation_for_correct_id()
        {
            new ReceiptId(Guid.NewGuid());
        }
    }
}
