using System;
using System.Collections.Generic;
using System.Linq;
using Billy.CodeReadability;

namespace Billy.PolishReceiptRecognitionAlgorithm
{
    public class ParsingResult<TItem>
    {
        public Option<TItem> Value { get; }

        public Option<string> RawValue { get; }

        public IReadOnlyCollection<string> Problems { get; }
        
        private ParsingResult(Option<TItem> value, Option<string> rawValue, IReadOnlyCollection<string> problems)
        {
            Value = value;
            RawValue = rawValue;
            Problems = problems;
        }

        public override string ToString()
        {
            var value = Value.Match(
                val => $"parsed - [{val}]",
                () => "parsed - []");

            var raw = RawValue.Match(
                val => $"raw - [{val}]",
                () => "raw - []");

            return $"{value}, {raw} (problems: {string.Join(", ", Problems)})";
        }

        //todo: probably too many of those factory methods...
        public static ParsingResult<string> WithoutProblems(string value) =>
            new ParsingResult<string>(
                Option<string>.Some(value),
                Option<string>.Some(value),
                new string[0]);

        public static ParsingResult<TItem> WithoutProblems(TItem value, string rawValue) =>
            new ParsingResult<TItem>(
                Option<TItem>.Some(value), 
                Option<string>.Some(rawValue),
                new string[0]);
        
        public static ParsingResult<TItem> WithProblems(Option<TItem> value, Option<string> rawValue, IEnumerable<string> problems) =>
            new ParsingResult<TItem>(
                value,
                rawValue,
                problems.ToArray());

        public static ParsingResult<TItem> WithProblems(TItem value, Option<string> rawValue, IEnumerable<string> problems) =>
            new ParsingResult<TItem>(
                Option<TItem>.Some(value),
                rawValue,
                problems.ToArray());

        public static ParsingResult<TItem> WithProblems(string rawValue, IEnumerable<string> problems) =>
            new ParsingResult<TItem>(
                Option<TItem>.None,
                Option<string>.Some(rawValue),
                problems.ToArray());
        
        public static ParsingResult<TItem> Empty(IEnumerable<string> problems) =>
            new ParsingResult<TItem>(
                Option<TItem>.None, 
                Option<string>.None, 
                problems.ToArray());

        public static ParsingResult<TItem> NotFound() =>
            new ParsingResult<TItem>(
                Option<TItem>.None,
                Option<string>.None,
                new [] {ParsingProblem.NotFound});
    }
}