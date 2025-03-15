using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Billy.CodeReadability;
using Billy.PolishReceiptRecognitionAlgorithm.Geometry;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;
using Billy.PolishReceiptRecognitionAlgorithm.ReceiptContentAnalysisAndCorrection;
using FluentAssertions;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests.Serialization
{
    public class ReceiptSerializationTests
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
        public void can_serialize_receipt()
        {
            //given
            var receipt = new Receipt(
                seller: BoxedParsingResult<string>.WithoutProblems("seller", "seller raw", BoundingBox()),
                taxNumber: BoxedParsingResult<TaxNumber>.WithoutProblems(new TaxNumber("0123456789"), "nip 0123456789", BoundingBox()),
                date: BoxedParsingResult<DateTime>.WithoutProblems(new DateTime(1991, 2, 7), "1991-02-07", BoundingBox()),
                amount: BoxedParsingResult<decimal>.WithoutProblems(100m, "amount 100.00", BoundingBox()),
                products: new Either<RecognizedReceiptProduct, UnrecognizedReceiptProduct>[]
                {
                    new RecognizedReceiptProduct(
                        name: ParsingResult<string>.WithoutProblems("name", "raw name"),
                        quantity: ParsingResult<decimal>.NotFound(),
                        unit: ParsingResult<string>.NotFound(),
                        unitPrice: ParsingResult<decimal>.NotFound(),
                        amount: ParsingResult<decimal>.WithoutProblems(120m, "120"),
                        taxTag: ParsingResult<string>.NotFound(),
                        boundingBox: BoundingBox()),

                    new UnrecognizedReceiptProduct(
                        text: "some unrecognized product", 
                        boundingBox: BoundingBox())
                },
                problems: new List<string>()
                {
                    ReceiptProblem.AmountDifferentThanSumOfProducts,
                },
                originalOrientation: new Receipt.Orientation(0));

            //when

            var json = JsonSerializer.Serialize(receipt, _serializerOptions);

            //then
            json.Should().Be(
                "{" +
                "\"OriginalOrientation\":{" +
                "\"ValueInRadians\":0" +
                "}," +
                "\"Seller\":{" +
                "\"Value\":{" +
                "\"hasItem\":true," +
                "\"item\":\"seller\"" +
                "}," +
                "\"RawValue\":{" +
                "\"hasItem\":true," +
                "\"item\":\"seller raw\"" +
                "}," +
                "\"Problems\":[]," +
                "\"BoundingBox\":{" +
                "\"LeftTop\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}," +
                "\"RightTop\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}," +
                "\"LeftBottom\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}," +
                "\"RightBottom\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}" +
                "}" +
                "}," +
                "\"TaxNumber\":{" +
                "\"Value\":{" +
                "\"hasItem\":true," +
                "\"item\":{" +
                "\"Value\":\"0123456789\"" +
                "}" +
                "}," +
                "\"RawValue\":{" +
                "\"hasItem\":true," +
                "\"item\":\"nip 0123456789\"" +
                "}," +
                "\"Problems\":[]," +
                "\"BoundingBox\":{" +
                "\"LeftTop\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}," +
                "\"RightTop\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}," +
                "\"LeftBottom\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}," +
                "\"RightBottom\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}" +
                "}" +
                "}," +
                "\"Date\":{" +
                "\"Value\":{" +
                "\"hasItem\":true," +
                "\"item\":\"1991-02-07T00:00:00\"" +
                "}," +
                "\"RawValue\":{" +
                "\"hasItem\":true," +
                "\"item\":\"1991-02-07\"" +
                "}," +
                "\"Problems\":[]," +
                "\"BoundingBox\":{" +
                "\"LeftTop\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}," +
                "\"RightTop\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}," +
                "\"LeftBottom\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}," +
                "\"RightBottom\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}" +
                "}" +
                "}," +
                "\"Amount\":{" +
                "\"Value\":{" +
                "\"hasItem\":true," +
                "\"item\":100" +
                "}," +
                "\"RawValue\":{" +
                "\"hasItem\":true," +
                "\"item\":\"amount 100.00\"" +
                "}," +
                "\"Problems\":[]," +
                "\"BoundingBox\":{" +
                "\"LeftTop\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}," +
                "\"RightTop\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}," +
                "\"LeftBottom\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}," +
                "\"RightBottom\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}" +
                "}" +
                "}," +
                "\"Products\":[" +
                "{" +
                "\"isLeft\":true," +
                "\"left\":{" +
                "\"Name\":{" +
                "\"Value\":{" +
                "\"hasItem\":true," +
                "\"item\":\"name\"" +
                "}," +
                "\"RawValue\":{" +
                "\"hasItem\":true," +
                "\"item\":\"raw name\"" +
                "}," +
                "\"Problems\":[]" +
                "}," +
                "\"Unit\":{" +
                "\"Value\":{" +
                "\"hasItem\":false," +
                "\"item\":null" +
                "}," +
                "\"RawValue\":{" +
                "\"hasItem\":false," +
                "\"item\":null" +
                "}," +
                "\"Problems\":[" +
                "\"not_found\"" +
                "]" +
                "}," +
                "\"UnitPrice\":{" +
                "\"Value\":{" +
                "\"hasItem\":false," +
                "\"item\":null" +
                "}," +
                "\"RawValue\":{" +
                "\"hasItem\":false," +
                "\"item\":null" +
                "}," +
                "\"Problems\":[" +
                "\"not_found\"" +
                "]" +
                "}," +
                "\"Quantity\":{" +
                "\"Value\":{" +
                "\"hasItem\":false," +
                "\"item\":null" +
                "}," +
                "\"RawValue\":{" +
                "\"hasItem\":false," +
                "\"item\":null" +
                "}," +
                "\"Problems\":[" +
                "\"not_found\"" +
                "]" +
                "}," +
                "\"Amount\":{" +
                "\"Value\":{" +
                "\"hasItem\":true," +
                "\"item\":120" +
                "}," +
                "\"RawValue\":{" +
                "\"hasItem\":true," +
                "\"item\":\"120\"" +
                "}," +
                "\"Problems\":[]" +
                "}," +
                "\"TaxTag\":{" +
                "\"Value\":{" +
                "\"hasItem\":false," +
                "\"item\":null" +
                "}," +
                "\"RawValue\":{" +
                "\"hasItem\":false," +
                "\"item\":null" +
                "}," +
                "\"Problems\":[" +
                "\"not_found\"" +
                "]" +
                "}," +
                "\"BoundingBox\":{" +
                "\"LeftTop\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}," +
                "\"RightTop\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}," +
                "\"LeftBottom\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}," +
                "\"RightBottom\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}" +
                "}" +
                "}," +
                "\"right\":null" +
                "}," +
                "{" +
                "\"isLeft\":false," +
                "\"left\":null," +
                "\"right\":{" +
                "\"RawText\":\"some unrecognized product\"," +
                "\"BoundingBox\":{" +
                "\"LeftTop\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}," +
                "\"RightTop\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}," +
                "\"LeftBottom\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}," +
                "\"RightBottom\":{" +
                "\"X\":0," +
                "\"Y\":0" +
                "}" +
                "}" +
                "}" +
                "}" +
                "]," +
                "\"Problems\":[" +
                "\"amount_different_than_sum_of_products\"" +
                "]" +
                "}"
            );
        }

        public BoundingBox BoundingBox()
        {
            return new BoundingBox(
                new Point(0,0),
                new Point(0,0),
                new Point(0,0),
                new Point(0,0));
        }
    }
}
