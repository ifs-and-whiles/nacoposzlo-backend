using System;
using System.Collections.Generic;
using System.Text;
using Billy.Domain;

namespace Billy.Receipts.Domain
{
    public class StorageName : Value<StorageName>
    {
        public string Value { get; }

        public StorageName(string value)
        {
            Value = value;
        }

        public static StorageName From(string value) =>
            new StorageName(value);
    }
}
