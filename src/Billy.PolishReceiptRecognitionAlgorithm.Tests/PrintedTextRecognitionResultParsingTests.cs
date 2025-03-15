using System;
using System.Collections.Generic;
using System.Linq;
using Billy.CodeReadability;
using Billy.PolishReceiptRecognitionAlgorithm.Geometry;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;
using Billy.PolishReceiptRecognitionAlgorithm.Tests.Infrastructure;
using Billy.PolishReceiptRecognitionAlgorithm.Tests.Receipts;
using FluentAssertions;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests
{
    public class PrintedTextRecognitionResultParsingTests
    {
        [Theory]
        [InlineData("receipt_id_22.json")]
        [InlineData("receipt_id_23.json")]
        //[InlineData("receipt_id_24.json")]
        //[InlineData("receipt_id_25.json")]
        [InlineData("receipt_id_26.json")]
        public void can_read_unified_ocr(string fileName)
        {
            //given
            var ocr = ReceiptJsonReader.Read<PrintedTextRecognitionResult>(
                $"Receipts/PrintedTextRecognitionResult/{fileName}");

            //when
            var ocrResult = ToOcrResult(ocr);

            var receipt = ReceiptParser.Parse(ocrResult);

            //then
            var expected = TestReceipts.ExpectedReceipt(fileName);

            receipt.Match(
                parsedReceipt =>
                {
                    parsedReceipt.Amount.Should().BeEquivalentTo(expected.Amount);
                    parsedReceipt.Date.Should().BeEquivalentTo(expected.Date);
                    parsedReceipt.Seller.Should().BeEquivalentTo(expected.Seller);
                    parsedReceipt.TaxNumber.Should().BeEquivalentTo(expected.TaxNumber);

                    parsedReceipt.Products
                        .Where(p => p.TryGetLeft(out _))
                        .Select(
                            l => l,
                            r => throw new InvalidOperationException("Unrecognized products should not be part of the tests"))
                        .ToArray().Should().BeEquivalentTo(
                            expected.Products, 
                            opt => opt.Excluding(x=>x.BoundingBox)); //todo in future should not exclude boundingbox - it should be provided for expected data set as well
                },
                notParsed => throw new WrongResultException(
                    "Receipt should be parsed."));
        }

        private static OcrResult ToOcrResult(PrintedTextRecognitionResult ocr)
        {
            return new OcrResult(ocr.Detections.Select(x=>new RawDetectedText
            {
                Text = x.Text,
                Box =  new BoundingBox(x.LeftTop, x.RightTop, x.RightBottom, x.LeftBottom)
            }));
        }

        public class PrintedTextRecognitionResult
        {
            public IReadOnlyList<DetectedText> Detections { get; }

            public PrintedTextRecognitionResult(
                IEnumerable<DetectedText> detections)
            {
                Detections = detections.ToArray();
            }
        }

        public class DetectedText
        {
            public DetectedText(
                string text,
                Point leftTop,
                Point rightTop,
                Point leftBottom,
                Point rightBottom)
            {
                Text = text;
                LeftTop = leftTop;
                RightTop = rightTop;
                LeftBottom = leftBottom;
                RightBottom = rightBottom;
            }
            public string Text { get; }
            public Point LeftTop { get; }
            public Point RightTop { get; }
            public Point LeftBottom { get; }
            public Point RightBottom { get; }
        }
    }
}