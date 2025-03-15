using System;
using System.Linq;
using Billy.CodeReadability;
using Billy.PolishReceiptRecognitionAlgorithm.Geometry;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;
using Billy.PolishReceiptRecognitionAlgorithm.Tests.Infrastructure;
using Billy.PolishReceiptRecognitionAlgorithm.Tests.Receipts;
using FluentAssertions;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Xunit;
using OcrResult = Billy.PolishReceiptRecognitionAlgorithm.OcrJson.OcrResult;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests
{
    public class AzureOcrParsingTests
    {
        [Theory]
        [InlineData("receipt_id_14.json")]
        [InlineData("receipt_id_15.json")]
        [InlineData("receipt_id_16.json")]
        [InlineData("receipt_id_18.json")]
        [InlineData("receipt_id_19.json")]
        [InlineData("receipt_id_20.json")]
        public void can_read_azure_ocr(string fileName)
        {
            //given
            var ocr = ReceiptJsonReader
                .Read<TextRecognitionResult>(
                    $"Receipts/AzureVisionAPI/{fileName}");

            var unifiedOcr = ToUnifiedOcr(ocr);

            //when
            var receipt = ReceiptParser.Parse(unifiedOcr);

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
                            l => (IReceiptProduct)l,
                            r => throw new InvalidOperationException("Unrecognized products should not be part of the tests"))
                        .ToArray().Should().BeEquivalentTo(
                            expected.Products,
                            opt => opt.Excluding(x => x.BoundingBox)); //todo in future should not exclude boundingbox - it should be provided for expected data set as well
                },
                notParsed => throw new WrongResultException(
                    "Receipt should be parsed."));
        }

        private static OcrResult ToUnifiedOcr(TextRecognitionResult ocr)
        {
            return new OcrResult(ocr
                    .Lines
                    .Select(line => new RawDetectedText()
                    {
                        Text = line.Text,
                        Box = new BoundingBox(
                            leftTop: new Point(
                                x: line.BoundingBox[0],
                                y: line.BoundingBox[1]),
                            rightTop: new Point(
                                x: line.BoundingBox[2],
                                y: line.BoundingBox[3]),
                            rightBottom: new Point(
                                x: line.BoundingBox[4],
                                y: line.BoundingBox[5]),
                            leftBottom: new Point(
                                x: line.BoundingBox[6],
                                y: line.BoundingBox[7]))
                    })
                    .ToArray());
        }
    }
}