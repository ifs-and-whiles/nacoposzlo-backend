using System;
using System.Collections.Generic;
using System.Text;
using Billy.Domain;

namespace Billy.Receipts.Domain
{
    public class AlgorithmName : Value<AlgorithmName>
    {
        public string Value { get; }

        public AlgorithmName(string value)
        {
            Validate(value);
            Value = value;
        }

        private void Validate(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
                throw new DomainException("Receipt recognition algorithm name cannot be empty or white space", ErrorCodes.AlgorithmNameCanNotBeEmpty);
        }

        public override string ToString()
        {
            return Value;
        }

        public static AlgorithmName From(string name) =>
            new AlgorithmName(name);
    }
}
