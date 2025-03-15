using System;
using Billy.Domain;

namespace Billy.Receipts.Domain
{
    public class ReceiptId : Value<ReceiptId>
    {
        public Guid Value { get; }

        public ReceiptId(Guid value)
        {
            Value = value;
        }

        public static ReceiptId From(Guid value) =>
            new ReceiptId(value);
        
        public static implicit operator Guid(ReceiptId self) => self.Value;
    }
}