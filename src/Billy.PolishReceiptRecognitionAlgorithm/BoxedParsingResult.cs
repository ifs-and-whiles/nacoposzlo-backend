using System.Collections.Generic;
using System.Linq;
using Billy.CodeReadability;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;

namespace Billy.PolishReceiptRecognitionAlgorithm
{
    public class BoxedParsingResult<TItem>
    {
        public Option<TItem> Value { get; }

        public Option<string> RawValue { get; }

        public IReadOnlyCollection<string> Problems { get; }

        public BoundingBox BoundingBox { get; }

        private BoxedParsingResult(
            Option<TItem> value, 
            Option<string> rawValue, 
            BoundingBox boundingBox, 
            IReadOnlyCollection<string> problems)
        {
            Value = value;
            RawValue = rawValue;
            BoundingBox = boundingBox;
            Problems = problems;
        }

        public static BoxedParsingResult<string> WithoutProblems(string value, BoundingBox boundingBox) =>
          new BoxedParsingResult<string>(
              Option<string>.Some(value),
              Option<string>.Some(value),
              boundingBox,
              new string[0]);

        public static BoxedParsingResult<TItem> WithoutProblems(TItem value, string rawValue, BoundingBox boundingBox) =>
            new BoxedParsingResult<TItem>(
                Option<TItem>.Some(value),
                Option<string>.Some(rawValue),
                boundingBox,
                new string[0]);

        public static BoxedParsingResult<TItem> WithProblems(string rawValue, BoundingBox boundingBox, IEnumerable<string> problems) =>
            new BoxedParsingResult<TItem>(
                Option<TItem>.None,
                Option<string>.Some(rawValue),
                boundingBox,
                problems.ToArray());

        public static BoxedParsingResult<TItem> NotFound() =>
            new BoxedParsingResult<TItem>(
                Option<TItem>.None,
                Option<string>.None,
                null,
                new[] { ParsingProblem.NotFound });


        public static BoxedParsingResult<TItem> ExceptionOccured =>
            new BoxedParsingResult<TItem>(
                Option<TItem>.None,
                Option<string>.None,
                null,
                new[] { ParsingProblem.ExceptionOccured });
    }
}