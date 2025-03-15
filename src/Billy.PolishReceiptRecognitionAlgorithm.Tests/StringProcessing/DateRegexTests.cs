using System;
using System.Collections.Generic;
using System.Text;
using Billy.PolishReceiptRecognitionAlgorithm.Grammar;
using Billy.PolishReceiptRecognitionAlgorithm.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests.StringProcessing
{
    public class DateRegexTests
    {
        public static IEnumerable<object[]> ParsingDateTestData => new List<object[]>
        {
            new object[] {"2019-01-02", new DateTime(2019, 1, 2)},
            new object[] {"2019-01-02 15:20", new DateTime(2019, 1, 2, 15, 20, 0)},
            new object[] {"2019 - 01 - 02 15 : 20", new DateTime(2019, 1, 2, 15, 20, 0)},
            new object[] {"02-01-2019", new DateTime(2019, 1, 2)},
            new object[] {"02-01-2019 15:20", new DateTime(2019, 1, 2, 15, 20, 0)},
            new object[] {"02 - 01 - 2019 15 : 20", new DateTime(2019, 1, 2, 15, 20, 0)},
            new object[] {"2019-01-02 some other text in receipt line", new DateTime(2019, 1, 2)},
            new object[] {"19r09.28", new DateTime(2019, 9, 28)},
            new object[] {"dn.19r09.28 some other text in receipt line", new DateTime(2019, 9, 28)},
            new object[] {"25.10.2019 18:02", new DateTime(2019, 10, 25, 18, 2, 0)},
        };

        [Theory]
        [MemberData(nameof(ParsingDateTestData))]
        public void can_parse_date(string rawDate, DateTime expectedDate)
        {
            DateRegex.TryMatch(rawDate).Match(
                value => value.Should().Be(expectedDate),
                () => throw new WrongResultException("Should parse date correctly"));
        }

        [Theory]
        [InlineData("line without a date")]
        [InlineData("line with invalid month 2019-13-01")]
        [InlineData("line with invalid day 2019-01-32")]
        [InlineData("line with invalid hour 2019-01-01 25:15")]
        [InlineData("line with invalid minute 2019-01-01 12:61")]
        public void when_line_does_not_contain_valid_date_date_regex_should_return_none(string rawDate)
        {
            DateRegex.TryMatch(rawDate).Match(
                value => throw new WrongResultException("Should not be able to parse any date"),
                () => { });
        }

        [Fact]
        public void can_find_first_line_with_date()
        {
            //given
            var lines = new[]
            {
                "line without a date",
                "line with invalid month 2019-13-01",
                "line with invalid day 2019-01-32",
                "line with invalid hour 2019-01-01 25:15",
                "line with invalid minute 2019-01-01 12:61",
                "finally line with valid date 2019-01-02",
                "again line without a date"
            };

            //when
            var result = DateRegex.TryFindLine(lines);

            //then
            if (result.TryGet(out var item))
            {
                item.Index.Should().Be(new CodeReadability.Index(5));
                item.Value.Should().Be(new DateTime(2019, 1, 2));
            }
            else
            {
                throw new InvalidOperationException(
                    "Should not enter here - date was not found as expected");
            }
        }
    }
}
