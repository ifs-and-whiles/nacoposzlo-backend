using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Billy.CodeReadability;
using FluentAssertions;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests.Serialization
{
    public class ParsingResultSerializationTests
    {
        private JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            Converters =
            {
                new EitherJsonConverterFactory(),
                new OptionJsonConverterFactory()
            }
        };
        
        [Fact]
        public void can_serialize_parsing_result_without_problems()
        {
            //given
            var parsingResult = ParsingResult<string>
                .WithoutProblems("some value", "some raw value");

            //when
            var json = JsonSerializer.Serialize(parsingResult, _serializerOptions);

            //then
            json.Should().Be(
                "{\"Value\":{\"hasItem\":true,\"item\":\"some value\"}," +
                "\"RawValue\":{\"hasItem\":true,\"item\":\"some raw value\"}," +
                "\"Problems\":[]}");
        }

        [Fact]
        public void can_serialize_parsing_result_with_problems()
        {
            //given
            var parsingResult = ParsingResult<string>
                .WithProblems("some value", "some raw value", new string[]
                {
                    ParsingProblem.NotFound,
                    ParsingProblem.CalculatedBasedOnOtherValues,
                    ParsingProblem.UnexpectedCharactersFound,
                });

            //when
            var json = JsonSerializer.Serialize(parsingResult, _serializerOptions);

            //then
            json.Should().Be(
                "{\"Value\":{\"hasItem\":true,\"item\":\"some value\"}," +
                "\"RawValue\":{\"hasItem\":true,\"item\":\"some raw value\"}," +
                $"\"Problems\":[\"{ParsingProblem.NotFound}\"," +
                $"\"{ParsingProblem.CalculatedBasedOnOtherValues}\"," +
                $"\"{ParsingProblem.UnexpectedCharactersFound}\"]}}");
        }
    }
}
