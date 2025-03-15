using System;
using Billy.CodeReadability;
using Billy.EventSourcing;

namespace Billy.Expenses.Domain.Expenses
{
    public class ExpenseSeller : Value<ExpenseSeller>
    {
        public Option<SellerTaxNumber> TaxNumber { get; }
        public Option<SellerLocation> Location { get; }
        public Option<SellerName> Name { get; }
        public Option<SellerPostalCode> PostalCode { get; }

        public ExpenseSeller(
            Option<SellerTaxNumber> taxNumber, 
            Option<SellerLocation> location, 
            Option<SellerName> name, 
            Option<SellerPostalCode> postalCode)
        {
            TaxNumber = taxNumber;
            Location = location;
            Name = name;
            PostalCode = postalCode;
        }


        public static ExpenseSeller From(string taxNumber, string location, string name, string postalCode) =>
            new ExpenseSeller(
                SellerTaxNumber.From(taxNumber),
                SellerLocation.From(location),
                SellerName.From(name),
                SellerPostalCode.From(postalCode));

    }

    public class SellerTaxNumber : Value<SellerTaxNumber>
    {
        public string Value { get; }

        public SellerTaxNumber(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
        public static Option<SellerTaxNumber> From(string value) => string.IsNullOrWhiteSpace(value)
            ? Option<SellerTaxNumber>.None
            : new SellerTaxNumber(value);
    }

    public class SellerLocation : Value<SellerLocation>
    {
        public string Value { get; }

        public SellerLocation(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
        public static Option<SellerLocation> From(string value) => string.IsNullOrWhiteSpace(value)
            ? Option<SellerLocation>.None
            : new SellerLocation(value);
    }

    public class SellerName : Value<SellerName>
    {
        public string Value { get; }

        public SellerName(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
        
        public static Option<SellerName> From(string value) => string.IsNullOrWhiteSpace(value)
            ? Option<SellerName>.None
            : new SellerName(value);
    }

    public class SellerPostalCode : Value<SellerPostalCode>
    {
        public string Value { get; }

        public SellerPostalCode(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static Option<SellerPostalCode> From(string value) => string.IsNullOrWhiteSpace(value)
            ? Option<SellerPostalCode>.None
            : new SellerPostalCode(value);
    }

    public static class ExpenseSellerExtensions
    {
        public static string ValueOrNull(this Option<SellerTaxNumber> taxNumber) =>
            taxNumber.Match(value => value.Value, () => null);

        public static string ValueOrNull(this Option<SellerName> name) =>
            name.Match(value => value.Value, () => null);

        public static string ValueOrNull(this Option<SellerLocation> location) =>
            location.Match(value => value.Value, () => null);

        public static string ValueOrNull(this Option<SellerPostalCode> postalCode) =>
            postalCode.Match(value => value.Value, () => null);
    }
}
