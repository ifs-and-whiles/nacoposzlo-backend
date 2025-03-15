using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Billy.CodeReadability;

namespace Billy.PolishReceiptRecognitionAlgorithm.ReceiptContentAnalysisAndCorrection
{
    public static class ReceiptPostProcessor
    {
        private static readonly IReceiptProductFix[] ProductFixes =
        {
            new TooManyDecimalPlacesInProductAmountFix(), 
        };

        private static readonly IReceiptNonProductFilter[] NonProductFilters =
        {
            new KnownNonProductsFilter(),
            new RubbishFilter(), 
        };

        private static readonly IReceiptAnalyzer[] Analyzers =
        {
            new ReceiptAmountVsReceiptProductAmountsSumComparison(),
        };

        public static Receipt TryAnalyzeAndFix(Receipt original)
        {
            var fixedProducts = TryFixProducts(original);
            var filteredProducts = FilterOutNonProducts(fixedProducts);
            
            //todo refactor that. problems detection should go after products filtering
            var receiptWithFilteredProducts = new Receipt(
                seller: original.Seller,
                taxNumber: original.TaxNumber,
                date: original.Date,
                amount: original.Amount,
                products: filteredProducts,
                problems: original.Problems.ToArray(),
                originalOrientation:original.OriginalOrientation);

            var detectedProblems = TryDetectProblems(receiptWithFilteredProducts);
            
            return new Receipt(
                seller: original.Seller,
                taxNumber: original.TaxNumber,
                date: original.Date,
                amount: original.Amount,
                products: filteredProducts,
                problems: original.Problems.Concat(detectedProblems).ToArray(),
                originalOrientation: original.OriginalOrientation);
        }

        private static Either<RecognizedReceiptProduct, UnrecognizedReceiptProduct>[] TryFixProducts(Receipt original)
        {
            return original
                .Products
                .Select(ApplyProductFixes)
                .ToArray();
        }

        private static Either<RecognizedReceiptProduct, UnrecognizedReceiptProduct> ApplyProductFixes(
            Either<RecognizedReceiptProduct, UnrecognizedReceiptProduct> product)
        {
            return product.Select(
                recognized => ProductFixes.Aggregate(
                    seed: recognized,
                    func: (productAcc, fix) => fix.TryFix(productAcc)),
                unrecognized => unrecognized);
        }

        private static string[] TryDetectProblems(Receipt original)
        {
            return Analyzers
                .SelectMany(analyzer => analyzer.AnalyzeReceipt(original))
                .ToArray();
        }

        private static Either<RecognizedReceiptProduct, UnrecognizedReceiptProduct>[] FilterOutNonProducts(
            IEnumerable<Either<RecognizedReceiptProduct, UnrecognizedReceiptProduct>> products)
        {
            return products
                .Where(product => !IsNonProduct(product))
                .ToArray();
        }

        private static bool IsNonProduct(Either<RecognizedReceiptProduct, UnrecognizedReceiptProduct> product)
        {
            var isNonProduct = product.Match(
                recognized => IsNameMatchingNonProductCriteria(recognized.Name.Value),
                unrecognized => !string.IsNullOrEmpty(unrecognized.RawText) &&
                                IsNameMatchingNonProductCriteria(
                                    Option<string>.From(unrecognized.RawText)));

            return isNonProduct;
        }

        private static bool IsNameMatchingNonProductCriteria(Option<string> productName)
        {
            foreach (var nonProductFilter in NonProductFilters)
            {
                var isNonProduct = nonProductFilter.IsNonProduct(productName);

                if (isNonProduct) return true;
            }

            return false;
        }
    }
}
