using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Billy.EventSourcing
{
    public static class ValueObjectValidation
    {
        public static bool IsGreaterOrEqualThan<T>(this T value, T toCompare) where T : IComparable =>
            value.CompareTo(toCompare) >= 0;

        public static bool IsGreaterThan<T>(this T value, T toCompare) where T : IComparable =>
            value.CompareTo(toCompare) > 0;

        public static bool IsLessOrEqualThan<T>(this T value, T toCompare) where T : IComparable =>
            value.CompareTo(toCompare) <= 0;

        public static bool IsLessThan<T>(this T value, T toCompare) where T : IComparable =>
            value.CompareTo(toCompare) < 0;

        public static bool IsDefault<T>(this T value) where T : IComparable =>
            IsEqual(value, default);

        public static bool IsEqual<T>(this T value, T toCompare) where T : IComparable =>
            value.Equals(toCompare);
    }
}
