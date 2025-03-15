using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Billy.CodeReadability;
using Billy.PolishReceiptRecognitionAlgorithm.Tests.Receipts;
using Newtonsoft.Json;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests.Infrastructure
{
    public class ExpectedReceiptTestDataBuilder
    {
        public static string BoundingBoxes(Receipt receipt)
        {
            var boxes = receipt
                .Products
                .Select(p => p.BoundingBox, up => up.BoundingBox)
                .ToArray();

            return JsonConvert.SerializeObject(boxes);
        }

        public static string Prepare(Receipt receipt)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"new {nameof(ExpectedReceipt)}(");

            AppendProducts(builder, receipt);
            AppendSeller(builder, receipt);
            AppendDate(builder, receipt);
            AppendTaxNumber(builder, receipt);
            AppendAmount(builder, receipt);

            builder.AppendLine(")");

            return builder.ToString();
        }

        private static void AppendProducts(StringBuilder builder, Receipt receipt)
        {
            builder.AppendLine("products: new []");
            builder.AppendLine("{");

            var index = 0;

            foreach (var product in receipt.Products)
            {
                product.Match(
                    recognized => AppendProduct(builder, recognized, index == receipt.Products.Count),
                    unrecognized => throw new InvalidOperationException("Product was not recognized"));

                index++;
            }

            builder.AppendLine("},");
        }

        private static void AppendProduct(
            StringBuilder builder, 
            RecognizedReceiptProduct product,
            bool isLast)
        {
            var line = "Product(";

            product.Name.RawValue.Match(
                name => line += "\"" + name + "\", ",
                () => throw new InvalidOperationException("Product name was not found."));

            product.Quantity.RawValue.Match(
                quantity => line += "\"" + quantity + "\", ",
                () => line += "null, ");

            product.UnitPrice.RawValue.Match(
                unitPrice => line += "\"" + unitPrice + "\", ",
                () => line += "null, ");

            product.Amount.RawValue.Match(
                amount => line += "\"" + amount + "\", ",
                () => line += "null, ");

            product.TaxTag.RawValue.Match(
                taxTag => line += "\"" + taxTag + "\", ",
                () => line += "null, ");

            product.Unit.RawValue.Match(
                unit => line += "\"" + unit + "\"",
                () => line += "null");


            line += isLast ? ")" : "),";

            builder.AppendLine(line);
        }

        private static void AppendSeller(StringBuilder builder, Receipt receipt)
        {
            var seller = receipt
                .Seller
                .RawValue
                .Match(
                    value => value,
                    () => throw new InvalidOperationException("Seller was not found"));

            builder.AppendLine($"seller: Seller(\"{seller}\"),");
        }

        private static void AppendDate(StringBuilder builder, Receipt receipt)
        {
            var date = receipt
                .Date
                .Value
                .Match(
                    value => value,
                    () => throw new InvalidOperationException("Date was not found"));

            var dateRaw = receipt
                .Date
                .RawValue
                .Match(
                    value => value,
                    () => throw new InvalidOperationException("Raw date was not found"));

            builder.AppendLine($"date: Date(\"{ParseDate(date)}\",\"{dateRaw}\"),");
        }

        private static string ParseDate(DateTime date)
        {
            return $"{date.Day.ToString().PadLeft(2, '0')}.{date.Month.ToString().PadLeft(2, '0')}.{date.Year}";
        }

        private static void AppendTaxNumber(StringBuilder builder, Receipt receipt)
        {
            var taxNumber = receipt
                .TaxNumber
                .RawValue
                .Match(
                    value => value,
                    () => throw new InvalidOperationException("TaxNumber was not found"));

            builder.AppendLine($"taxNumber: TaxNumber(\"{taxNumber}\"),");
        }

        private static void AppendAmount(StringBuilder builder, Receipt receipt)
        {
            var amount = receipt
                .Amount
                .Value
                .Match(
                    value => value,
                    () => throw new InvalidOperationException("Amount was not found"));

            var amountRaw = receipt
                .Amount
                .RawValue
                .Match(
                    value => value,
                    () => throw new InvalidOperationException("Raw amount was not found"));

            builder.AppendLine($"amount: Amount({amount}m, \"{amountRaw}\"),");
        }
    }
}
