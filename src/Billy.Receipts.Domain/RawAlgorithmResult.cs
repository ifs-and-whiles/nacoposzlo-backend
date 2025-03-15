using System;
using System.Collections.Generic;
using System.Text;
using Billy.Domain;

namespace Billy.Receipts.Domain
{
    public class RawAlgorithmResult : Value<RawAlgorithmResult>
    {
        public string Value { get; }

        public RawAlgorithmResult(string value)
        {
            Value = value;
        }

        public static RawAlgorithmResult From(string result) =>
            new RawAlgorithmResult(result);
    }
}
