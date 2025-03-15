using System;
using System.Collections.Generic;
using System.Text;
using Billy.Domain;

namespace Billy.Receipts.Domain
{
    public class OcrProviderName : Value<OcrProviderName>
    {
        public string Value { get; }

        public OcrProviderName(string value)
        {
            Value = value;
        }

        public static OcrProviderName From(string value) =>
            new OcrProviderName(value);

        public override string ToString()
        {
            return Value;
        }
    }
}
