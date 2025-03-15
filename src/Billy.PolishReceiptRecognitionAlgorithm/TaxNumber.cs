using System;
using System.Linq;

namespace Billy.PolishReceiptRecognitionAlgorithm
{
    public class TaxNumber
    {
        protected bool Equals(TaxNumber other)
        {
            return Value == other.Value;
        }
        
        public string Value { get; }

        public TaxNumber(string value)
        {
            if (value.Length != 10)
                throw new ArgumentException(
                    $"NIP should contain exactly 10 digits, but found: '{value}' (length: {value.Length})");

            var onlyNumbers = value.All(char.IsNumber);

            if (!onlyNumbers)
                throw new ArgumentException(
                    $"NIP should contain only numbers, but found: '{value}'");

            Value = value;
        }

        public override string ToString() => Value;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TaxNumber)obj);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }
    }
}