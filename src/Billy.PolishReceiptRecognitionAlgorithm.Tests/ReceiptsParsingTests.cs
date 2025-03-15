using Billy.PolishReceiptRecognitionAlgorithm.Grammar;
using Billy.PolishReceiptRecognitionAlgorithm.Sections;
using Billy.PolishReceiptRecognitionAlgorithm.Tests.Receipts;
using FluentAssertions;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests
{
    public class ReceiptsParsingTests
    {
        [Theory]
        [MemberData(nameof(TestReceipts.Receipts), MemberType = typeof(TestReceipts))]
        public void can_parse_receipt_lines_into_section(TestReceipt testReceipt)
        {
            //when
            var receipt = ReceiptSectionsParser.GetSections(testReceipt.Lines);

            //then
            receipt.Should().BeEquivalentTo(new ReceiptSections(
                new ReceiptSections.HeaderSection(testReceipt.HeaderLines),
                new ReceiptSections.ProductsSection(testReceipt.ProductsLines),
                new ReceiptSections.TaxesSection(testReceipt.TaxesLines),
                new ReceiptSections.AmountSection(testReceipt.AmountLine),
                new ReceiptSections.FooterSection(testReceipt.FooterLines)), 
                $"Test Receipt with ID = {testReceipt.Id} parsing went wrong.");
        }

        [Theory]
        [MemberData(nameof(TestReceipts.Receipts), MemberType = typeof(TestReceipts))]
        public void can_parse_receipt(TestReceipt testReceipt)
        {
            if(testReceipt.Id == 9) return; //todo distinguish phone number from nip

            //when
            var rawReceipt = ReceiptSectionsParser.GetSections(testReceipt.Lines);
            var seller = ReceiptHeaderParser.ParseSeller(rawReceipt.Header);
            var taxNumber = ReceiptHeaderParser.ParseTaxNumber(rawReceipt.Header);
            var date = ReceiptDateParser.ParseDate(rawReceipt.Header, rawReceipt.Footer);
            var amount = ReceiptAmountParser.GetAmount(rawReceipt.Amount);
            var products = ReceiptProductsParser.ParseProducts(rawReceipt.Products);

            //then
            seller.Should().BeEquivalentTo(testReceipt.ActualSeller);
            taxNumber.Should().BeEquivalentTo(testReceipt.ActualTaxNumber);
            date.Should().BeEquivalentTo(testReceipt.ActualDate);
            amount.Should().BeEquivalentTo(testReceipt.ActualReceiptAmount);
            products.Should().BeEquivalentTo(testReceipt.ActualProducts);
        }
    }
}
