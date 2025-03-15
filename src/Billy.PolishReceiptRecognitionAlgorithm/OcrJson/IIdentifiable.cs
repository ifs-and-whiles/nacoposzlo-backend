using System.Collections.Generic;

namespace Billy.PolishReceiptRecognitionAlgorithm.OcrJson
{
    public interface IIdentifiable
    {
        public int Id { get; }
    }

    public class IdentifiableEqualityComparer<T> : EqualityComparer<T> where T:IIdentifiable
    {
        public override bool Equals(T x, T y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Id == y.Id;
        }

        public override int GetHashCode(T obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}