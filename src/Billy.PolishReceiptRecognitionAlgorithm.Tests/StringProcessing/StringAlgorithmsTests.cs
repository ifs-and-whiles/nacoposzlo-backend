using System;
using System.Collections.Generic;
using Billy.PolishReceiptRecognitionAlgorithm.StringProcessing;
using FluentAssertions;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests.StringProcessing
{
    public class StringAlgorithmsTests
    {
        [Fact]
        public void distance_between_same_words_is_equal_to_0()
        {
            //when
            var distance = StringAlgorithm.TryMatchWithLevenshteinDistance("Paragon fiskalny", "Paragon fiskalny", 15);
            
            //then
            distance.Match(
                value =>
                {
                    value.LevenshteinDistance.Should().Be(0);
                    value.IsPerfectlyMatched.Should().BeTrue();
                },
                thresholdExceeded => throw new Exception("LevenshteinDistance exceeded threshold"));
        }

        [Fact]
        public void when_couple_of_letters_are_mistaken_distance_grows()
        {
            //when
            var distance = StringAlgorithm.TryMatchWithLevenshteinDistance("Paragon fiskalny", "faraqom liskafny", 15);

            //then
            distance.Match(
                value =>
                {
                    value.LevenshteinDistance.Should().BeGreaterThan(0);
                    value.IsPerfectlyMatched.Should().BeFalse();
                },
                thresholdExceeded => throw new Exception("LevenshteinDistance exceeded threshold"));
        }

        public static IEnumerable<object[]> ParsingDecimals() => new[]
        {
            new object[] {"120", 120M},
            new object[] {"120.95", 120.95M},
            new object[] {"120,95", 120.95M},
            new object[] {"00120,95", 120.95M},
            new object[] {"00120,9500", 120.95M},
        };

        [Theory]
        [MemberData(nameof(ParsingDecimals))]
        public void can_parse_decimal(string text, decimal expectedDecimal)
        {
            StringAlgorithm.ToDecimal(text).Match(
                value => value.Should().Be(expectedDecimal),
                notANumber => throw new Exception($"'{notANumber.Text}' is not a decimal"));
        }
    }
}