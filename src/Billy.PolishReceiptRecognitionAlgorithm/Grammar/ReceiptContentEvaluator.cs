using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Billy.CodeReadability;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;
using Billy.PolishReceiptRecognitionAlgorithm.StringProcessing;

namespace Billy.PolishReceiptRecognitionAlgorithm.Grammar
{
    //todo refactor
    public class ReceiptContentEvaluator : ExpressionBaseVisitor<object>
    {
        private readonly ReceiptLine[] _originalLines;
        private int _currentLineIndex = 0;

        public ReceiptContentEvaluator(
            IEnumerable<ReceiptLine> originalLines)
        {
            _originalLines = originalLines.ToArray();
        }

        public override object VisitProductsexp(ExpressionParser.ProductsexpContext context)
        {
            return base.Visit(context.products());
        }

        public override object VisitSingleLineProduct(ExpressionParser.SingleLineProductContext context)
        {
            var line = _originalLines[_currentLineIndex];
            _currentLineIndex++;

            var name = (ParsingResult<string>) 
                Visit(context.name());

            var (quantity, unit) = ((ParsingResult<decimal>, ParsingResult<string>)) 
                Visit(context.quantity());

            var unitPrice = (ParsingResult<decimal>) 
                Visit(context.unitPrice());

            var (amount, taxTag) = ((ParsingResult<decimal>, ParsingResult<string>))
                Visit(context.amount());

            return new RecognizedReceiptProduct(
                name, 
                quantity, 
                unit, 
                unitPrice, 
                amount, 
                taxTag,
                line.BoundingBox);
        }

        public override object VisitMultiLinesProduct(ExpressionParser.MultiLinesProductContext context)
        {
            var firstLine = _originalLines[_currentLineIndex];
            _currentLineIndex++;

            var secondLine = _originalLines[_currentLineIndex];
            _currentLineIndex++;

            var boundingBox = BoundingBox.GetBoundingBoxFor(
                new[] {firstLine.BoundingBox, secondLine.BoundingBox});

            var name = (ParsingResult<string>)
                Visit(context.name());

            var (quantity, unit) = ((ParsingResult<decimal>, ParsingResult<string>))
                Visit(context.quantity());

            var unitPrice = (ParsingResult<decimal>)
                Visit(context.unitPrice());

            var (amount, taxTag) = ((ParsingResult<decimal>, ParsingResult<string>))
                Visit(context.amount());

            return  new RecognizedReceiptProduct(
                name, 
                quantity, 
                unit, 
                unitPrice, 
                amount,
                taxTag,
                boundingBox);
        }

        public override object VisitSingleLineMissingQuantityProduct(ExpressionParser.SingleLineMissingQuantityProductContext context)
        {
            var line = _originalLines[_currentLineIndex];
            _currentLineIndex++;

            var name = (ParsingResult<string>)
                Visit(context.name());

            var unitPrice = (ParsingResult<decimal>)
                Visit(context.unitPrice());

            var (amount, taxTag) = ((ParsingResult<decimal>, ParsingResult<string>))
                Visit(context.amount());

            return new RecognizedReceiptProduct(
                name,
                ParsingResult<decimal>.NotFound(),
                ParsingResult<string>.NotFound(),
                unitPrice,
                amount,
                taxTag,
                line.BoundingBox);
        }

        public override object VisitMultiLinesMissingQuantityProduct(ExpressionParser.MultiLinesMissingQuantityProductContext context)
        {
            var firstLine = _originalLines[_currentLineIndex];
            _currentLineIndex++;

            var secondLine = _originalLines[_currentLineIndex];
            _currentLineIndex++;

            var boundingBox = BoundingBox.GetBoundingBoxFor(
                new[] { firstLine.BoundingBox, secondLine.BoundingBox });

            var firstLineName = (ParsingResult<string>)
                Visit(context.fln);

            var secondLineName = (ParsingResult<string>)
                Visit(context.sln);

            var unitPrice = (ParsingResult<decimal>)
                Visit(context.unitPrice());

            var (amount, taxTag) = ((ParsingResult<decimal>, ParsingResult<string>))
                Visit(context.amount());

            var name = firstLineName.Value.Add(secondLineName.Value.Select(val => " " + val));
            var rawName = firstLineName.RawValue.Add(secondLineName.RawValue.Select(val => " " + val));
            var problems = firstLineName.Problems.Concat(secondLineName.Problems).Distinct().ToArray();

            return new RecognizedReceiptProduct(
                ParsingResult<string>.WithProblems(name, rawName, problems),
                ParsingResult<decimal>.NotFound(),
                ParsingResult<string>.NotFound(),
                unitPrice,
                amount,
                taxTag,
                boundingBox);
        }

        public override object VisitSingleLineMissingQuantityAndUnitPriceProduct(
            ExpressionParser.SingleLineMissingQuantityAndUnitPriceProductContext context)
        {
            var line = _originalLines[_currentLineIndex];
            _currentLineIndex++;

            var name = (ParsingResult<string>)
                Visit(context.name());

            var (amount, taxTag) = ((ParsingResult<decimal>, ParsingResult<string>))
                Visit(context.amount());

            return new RecognizedReceiptProduct(
                name,
                ParsingResult<decimal>.NotFound(),
                ParsingResult<string>.NotFound(),
                ParsingResult<decimal>.NotFound(),
                amount,
                taxTag,
                line.BoundingBox);
        }

        public override object VisitProducts(ExpressionParser.ProductsContext context)
        {
            if (context.unrecognizedSequence() != null && context.singleLineProduct() != null)
            {
                var line = _originalLines[_currentLineIndex];
                _currentLineIndex++;

                var unrecognized = context.unrecognizedSequence().GetText();
                var product = (IReceiptProduct) Visit(context.singleLineProduct());

                return new[]
                {
                    new UnrecognizedReceiptProduct(
                        unrecognized,
                        line.BoundingBox), 
                    product
                };
            }

            if (context.singleLineProduct() != null)
                return new[] {(IReceiptProduct) Visit(context.singleLineProduct())};

            if (context.multiLinesProduct() != null)
                return new[] {(IReceiptProduct) Visit(context.multiLinesProduct())};

            if (context.singleLineMissingQuantityProduct() != null)
                return new[] {(IReceiptProduct) Visit(context.singleLineMissingQuantityProduct())};

            if (context.multiLinesMissingQuantityProduct() != null)
                return new[] {(IReceiptProduct) Visit(context.multiLinesMissingQuantityProduct())};

            if (context.singleLineMissingQuantityAndUnitPriceProduct() != null)
                return new[] {(IReceiptProduct) Visit(context.singleLineMissingQuantityAndUnitPriceProduct())};

            if (context.unrecognizedSequence() != null)
            {
                var line = _originalLines[_currentLineIndex];
                _currentLineIndex++;

                var text = context.unrecognizedSequence().GetText();
                return new[]
                {
                    new UnrecognizedReceiptProduct(
                        text,
                        line.BoundingBox)
                };
            }

            if (context.products() != null)
            {
                var products = context
                    .products()
                    .SelectMany(product => (IReceiptProduct[]) Visit(product))
                    .ToArray();

                return products;
            }

            throw new InvalidOperationException("There are no other Products possibility.");
        }

        public override object VisitName(ExpressionParser.NameContext context)
        {
            var name = context.GetText();
            return ParsingResult<string>.WithoutProblems(name);
        }

        public override object VisitQuantity(ExpressionParser.QuantityContext context)
        {
            var quantity = TryGetDecimal(context.qt);
            
            return context.unit() != null 
                ? (quantity, (ParsingResult<string>) Visit(context.unit())) 
                : (quantity, ParsingResult<string>.NotFound());
        }

        public override object VisitUnit(ExpressionParser.UnitContext context)
        {
            var unit = (string) Visit(context.word());

            return ParsingResult<string>.WithoutProblems(
                value: unit);
        }

        public override object VisitUnitPrice(ExpressionParser.UnitPriceContext context)
        {
            var unitPrice = TryGetDecimal(context.up);
            return unitPrice;
        }

        public override object VisitAmount(ExpressionParser.AmountContext context)
        {
            var amount = TryGetDecimal(context.am);

            if (context.taxTag != null)
            {
                var taxTag = (string) Visit(context.taxTag);

                if (taxTag.Length == 1)
                    return (amount, ParsingResult<string>.WithoutProblems(value: taxTag));
                else
                    return (amount, ParsingResult<string>.WithProblems(
                        value: taxTag,
                        rawValue: taxTag,
                        problems: new[]
                        {
                            ParsingProblem.TooManyCharactersFound
                        }));
            }

            return (amount, ParsingResult<string>.Empty(
                problems: new[] {ParsingProblem.NotFound}));
        }

        public override object VisitWord(ExpressionParser.WordContext context)
        {
            return context.GetText();
        }

        private static ParsingResult<decimal> TryGetDecimal(IToken token)
        {
            if (token != null)
                return StringAlgorithm
                    .ToDecimal(token.Text)
                    .Match(
                        value => ParsingResult<decimal>.WithoutProblems(value, token.Text),
                        notANumber => throw new InvalidOperationException(
                            $"Value '{notANumber.Text}' is not a number"));

            return ParsingResult<decimal>.Empty(
                problems: new[] { ParsingProblem.NotFound });
        }
    }
}
