using System;
using System.Collections.Generic;
using Billy.PolishReceiptRecognitionAlgorithm.Grammar;
using Billy.PolishReceiptRecognitionAlgorithm.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests.StringProcessing
{
    public class TaxNumberRegexTests
    {
        [Theory]
        [InlineData("NIP 111-111-11-11", "1111111111")]
        [InlineData("nip 111-111-11-11", "1111111111")]
        [InlineData("NIP 1111111111", "1111111111")]
        [InlineData("NIP1111111111", "1111111111")]
        [InlineData("NIP 1-1-1-1-1-1-1-1-1-1", "1111111111")]
        [InlineData("NIP 111 - 111---11 11", "1111111111")]
        [InlineData("    111 - 111---11 11", "1111111111")]
        [InlineData("    111 - 111---11 11 22", "1111111111")]
        public void can_parse_tax_number(string rawTaxNumber, string expectedTaxNumber)
        {
            TaxNumberRegex.TryMatch(rawTaxNumber).Match(
                value => value.Should().Be(expectedTaxNumber),
                () => throw new WrongResultException("Should parse tax number correctly"));
        }

        [Theory]
        [InlineData("NIP 111-111-11-11", "1111111111")]
        [InlineData("nip 111-111-11-11", "1111111111")]
        [InlineData("NIP 1111111111", "1111111111")]
        [InlineData("NIP1111111111", "1111111111")]
        [InlineData("NIP 1-1-1-1-1-1-1-1-1-1", "1111111111")]
        [InlineData("NIP 111 - 111---11 11", "1111111111")]
        public void can_parse_tax_number_with_prefix(string rawTaxNumber, string expectedTaxNumber)
        {
            TaxNumberRegex.TryMatchWithPrefix(rawTaxNumber).Match(
                value => value.Should().Be(expectedTaxNumber),
                () => throw new WrongResultException("Should parse tax number correctly"));
        }

        [Theory]
        [InlineData("line without a tax number")]
        [InlineData("line with invalid tax number NIP 111-abc-11-11")]
        public void when_line_does_not_contain_valid_tax_number_regex_should_return_none(string rawTaxNumber)
        {
            TaxNumberRegex.TryMatch(rawTaxNumber).Match(
                value => throw new WrongResultException("Should not be able to parse any tax number"),
                () => { });
        }

        [Fact]
        public void can_find_first_line_with_tax_number()
        {
            //given
            var lines = new[]
            {
                "line without a tax number",
                "line with invalid tax number NIP 111-abc-11-11",
                "finally line with valid tax number NIP 111-111-11-11",
                "again line without a tax number"
            };

            //when
            var result = TaxNumberRegex.TryFindLine(lines);

            //then
            if (result.TryGet(out var item))
            {
                item.Index.Should().Be(new CodeReadability.Index(2));
                item.Value.Should().Be("1111111111");
            }
            else
            {
                throw new InvalidOperationException(
                    "Should not enter here - tax number was not found as expected");
            }
        }

        [Fact]
        public void can_find_first_line_with_tax_number_even_it_its_merged_with_date_line()
        {
            //given
            var lines = new[]
            {
                "line without a tax number",
                "line with invalid tax number NIP 111-abc-11-11",
                "2020-12-20 15:20 NIP 111-111-11-11",
                "again line without a tax number"
            };

            //when
            var result = TaxNumberRegex.TryFindLine(lines);

            //then
            if (result.TryGet(out var item))
            {
                item.Index.Should().Be(new CodeReadability.Index(2));
                item.Value.Should().Be("1111111111");
            }
            else
            {
                throw new InvalidOperationException(
                    "Should not enter here - tax number was not found as expected");
            }
        }
    }
}