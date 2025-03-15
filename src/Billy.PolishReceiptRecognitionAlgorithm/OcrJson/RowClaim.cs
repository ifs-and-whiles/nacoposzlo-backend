using System;

namespace Billy.PolishReceiptRecognitionAlgorithm.OcrJson
{
    public class RowClaim<T> where T:IIdentifiable
    {
        public Pair<T> Items { get; }

        public double Strength { get; }

        public RowClaim(Pair<T> items, double strength)
        {
            Items = items;
            Strength = strength;
        }

        public override string ToString()
        {
            return $"{Items} ({Strength})";
        }

        protected bool Equals(RowClaim<T> other)
        {
            return Equals(Items, other.Items) && Strength.Equals(other.Strength);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RowClaim<T>)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Items, Strength);
        }
    }
}