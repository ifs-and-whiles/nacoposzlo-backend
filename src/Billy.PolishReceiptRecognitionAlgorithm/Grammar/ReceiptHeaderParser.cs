using System.Linq;
using Billy.CodeReadability;
using Billy.CollectionTools;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;
using Billy.PolishReceiptRecognitionAlgorithm.Sections;

namespace Billy.PolishReceiptRecognitionAlgorithm.Grammar
{
    public class ReceiptHeaderParser
    {
        public static BoxedParsingResult<string> ParseSeller(
            ReceiptSections.HeaderSection headerSection)
        {
            var sellerLine = TryExtractSellerLine(headerSection);

            return sellerLine.Match(
                line => BoxedParsingResult<string>.WithoutProblems(line.Text, line.BoundingBox),
                BoxedParsingResult<string>.NotFound);
        }

        private static Option<ReceiptLine> TryExtractSellerLine(
            ReceiptSections.HeaderSection headerSection)
        {
            /* Example receipt header lines:
            * 
            * "KLOS M.KRZYCA SP.JAWNA",
            * "40-664 KATOWICE, UL.NAPIERSKIEGO 43",
            * "SKLEP FIRMOWY - KAWIARENKA",
            * "NIP 954-264-02-26",
            * "2019-10-07 nr wydr.647997",
            *
            * the seller is always located in first line  of a receipt's header
            *
            */

            return headerSection.Lines.TryGetFirst();
        }

        public static BoxedParsingResult<TaxNumber> ParseTaxNumber(
            ReceiptSections.HeaderSection headerSection)
        {
            return TaxNumberRegex
                .TryFindLine(
                    headerSection.Lines.Select(line => line.Text).ToArray())
                .Match(
                    taxNumber =>
                    {
                        var correspondingLine = headerSection
                            .Lines
                            .ElementAt(taxNumber.Index);

                        return BoxedParsingResult<TaxNumber>.WithoutProblems(
                            new TaxNumber(taxNumber.Value),
                            correspondingLine.Text,
                            correspondingLine.BoundingBox);
                    },
                    BoxedParsingResult<TaxNumber>.NotFound);
        }
    }
}