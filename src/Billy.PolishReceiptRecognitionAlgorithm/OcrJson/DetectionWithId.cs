using System;
using System.Collections.Generic;

namespace Billy.PolishReceiptRecognitionAlgorithm.OcrJson
{
    public class DetectionWithId: IIdentifiable
    {
        public int Id { get; }
        public string Text { get; }
        public BoundingBox Box { get; }

        public DetectionWithId(
            int id,
            string text,
            BoundingBox box)
        {
            Id = id;
            Text = text;
            Box = box;
        }

        public override string ToString()
        {
            return Text;
        }
    }

    public class Pair<T> where T : IIdentifiable
    {
        public T First { get; }
        public T Second { get; }

        public T[] All => new [] {First, Second};
        public Pair(
            T a,
            T b)
        {
            if (a.Id == b.Id)
                throw new InvalidOperationException("Ids of items to pair cannot be equal");

            if (a.Id < b.Id)
            {
                First = a;
                Second = b;
            }
            else
            {
                First = b;
                Second = a;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Pair<T>)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(First.Id, Second.Id);
        }

        protected bool Equals(Pair<T> other)
        {
            return First.Id == other.First.Id && Second.Id == other.Second.Id;
        }

        public override string ToString()
        {
            return $"{First} -> {Second}";
        }

        public bool Contains(T item)
        {
            return First.Id == item.Id || Second.Id == item.Id;
        }
    }
}