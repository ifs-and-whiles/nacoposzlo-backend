using System.Linq;
using System.Threading;
using Billy.CodeReadability;
using Billy.PolishReceiptRecognitionAlgorithm.Geometry;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;
using Billy.PolishReceiptRecognitionAlgorithm.Tests.Infrastructure;
using Billy.PolishReceiptRecognitionAlgorithm.Tests.Receipts;
using FluentAssertions;
using Google.Cloud.Vision.V1;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests
{
    public class GoogleOcrParsingTests
    {
        [Theory]
        [InlineData("receipt_id_13_text_annotations.json")]
        //[InlineData("receipt_id_17_text_annotations.json")]
        public void can_parse_google_ocr(string fileName)
        {
            //given
            var annotations = ReceiptJsonReader.Read<EntityAnnotation[]>(
                    $"Receipts/GoogleVisionAPI/{fileName}");

            var unifiedOcr = ToUnifiedOcr(annotations);

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
                        .Select( 
                            l=>(IReceiptProduct) l, 
                            r => (IReceiptProduct) r)
                        .ToArray().Should().BeEquivalentTo(
                            expected.Products,
                            opt => opt.Excluding(x => x.BoundingBox)); //todo in future should not exclude boundingbox - it should be provided for expected data set as well);
                },
                notParsed => throw new WrongResultException(
                    "Receipt should be parsed."));
        }

        private static OcrResult ToUnifiedOcr(EntityAnnotation[] annotations)
        {
            return new OcrResult(annotations
                .Skip(1) //the first row contains whole text merged, but its useless for us
                .Select(annotation => new RawDetectedText()
                {
                    Text = annotation.Description,
                    Box = new BoundingBox(
                        leftTop: new Point(
                            x: annotation.BoundingPoly.Vertices[0].X,
                            y: annotation.BoundingPoly.Vertices[0].Y),
                        rightTop: new Point(
                            x: annotation.BoundingPoly.Vertices[1].X,
                            y: annotation.BoundingPoly.Vertices[1].Y),
                        rightBottom: new Point(
                            x: annotation.BoundingPoly.Vertices[2].X,
                            y: annotation.BoundingPoly.Vertices[2].Y),
                        leftBottom: new Point(
                            x: annotation.BoundingPoly.Vertices[3].X,
                            y: annotation.BoundingPoly.Vertices[3].Y))
                })
                .ToArray());
        }
    }
}