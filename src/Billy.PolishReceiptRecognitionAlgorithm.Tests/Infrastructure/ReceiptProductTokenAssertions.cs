using System;
using FluentAssertions;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests.Infrastructure
{
    public class ReceiptProductTokenAssertions<T>
    {
        private readonly ParsingResult<T> _token;
        private readonly string _tokenName;

        public ReceiptProductTokenAssertions(ParsingResult<T> token, string tokenName)
        {
            _token = token;
            _tokenName = tokenName;
        }

        public void ShouldBe(string expectedValue)
        {
            if (expectedValue == null)
            {
                ShouldBeEmpty();
            }
            else
            {
                _token.Value.Match(
                    value => value.Should().Be(expectedValue),
                    () => throw new Exception($"{_tokenName} should not be empty!"));
            }
        }

        public void ShouldBe(T expectedValue)
        {
            _token.Value.Match(
                value => value.Should().Be(expectedValue),
                () => throw new Exception($"{_tokenName} should not be empty!"));
        }
        
        public void ShouldBeEmpty()
        {
            if (_token.Value.TryGet(out var value))
            {
                throw new Exception($"{_tokenName} should be empty but found '{value}'");
            }
        }
    }
}