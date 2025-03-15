namespace Billy.PolishReceiptRecognitionAlgorithm.Tests.Infrastructure
{
    public class RecognizedReceiptProductAssertions
    {
        private readonly RecognizedReceiptProduct _product;

        public RecognizedReceiptProductAssertions(RecognizedReceiptProduct product)
        {
            _product = product;
        }

        public ReceiptProductTokenAssertions<string> Name() 
            => AssertionFor(_product.Name, nameof(_product.Name));

        public ReceiptProductTokenAssertions<decimal> Quantity()
            => AssertionFor(_product.Quantity, nameof(_product.Quantity));

        public ReceiptProductTokenAssertions<string> Unit()
            => AssertionFor(_product.Unit, nameof(_product.Unit));

        public ReceiptProductTokenAssertions<decimal> UnitPrice()
            => AssertionFor(_product.UnitPrice, nameof(_product.UnitPrice));

        public ReceiptProductTokenAssertions<decimal> Amount()
            => AssertionFor(_product.Amount, nameof(_product.Amount));

        public ReceiptProductTokenAssertions<string> TaxTag()
            => AssertionFor(_product.TaxTag, nameof(_product.TaxTag));

        private static ReceiptProductTokenAssertions<T> AssertionFor<T>(ParsingResult<T> token, string tokenName)
        {
            return new ReceiptProductTokenAssertions<T>(token, tokenName);
        }
    }
}