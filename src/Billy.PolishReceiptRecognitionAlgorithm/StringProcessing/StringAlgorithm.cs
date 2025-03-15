using System;
using System.Globalization;
using Billy.CodeReadability;

namespace Billy.PolishReceiptRecognitionAlgorithm.StringProcessing
{
    public static class StringAlgorithm
    {
        public static Either<decimal, NotANumber> ToDecimal(string text)
        {
            var textWithCorrectSeparator = text
                .Replace(",", ".")
                .Replace(" ", "");

            if (decimal.TryParse(
                textWithCorrectSeparator,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var value))
            {
                return value;
            }

            return new NotANumber(text);
        }

        /// <summary>
        /// Levenshtein distance algorithm measures how different two strings are (how distant they are).
        /// The bigger the distance the more different the strings are.
        /// For example: distance between "some value" and "some value" is equal to 0 (strings are identical)
        /// but distance between "some vaule" and "some other value" is greater than zero (string are different)
        /// </summary>
        /// <param name="first">First string to compare</param>
        /// <param name="second">Second string to compare</param>
        /// <param name="threshold">If distance between string is greater than given threshold calculations will be stopped.</param>
        /// <returns></returns>
        public static Either<LevenshteinMatchingResult, LevenshteinThresholdExceeded> TryMatchWithLevenshteinDistance(
            string first, string second, int threshold)
        {
            var length1 = first.Length;
            var length2 = second.Length;

            // Return trivial case - difference in string lengths exceeds threshhold
            var lengthDistance = Math.Abs(length1 - length2);
            if (lengthDistance > threshold) { return new LevenshteinThresholdExceeded(lengthDistance); }

            // Ensure arrays [i] / length1 use shorter length 
            if (length1 > length2)
            {
                Swap(ref second, ref first);
                Swap(ref length1, ref length2);
            }

            var maxi = length1;
            var maxj = length2;

            var dCurrent = new int[maxi + 1];
            var dMinus1 = new int[maxi + 1];
            var dMinus2 = new int[maxi + 1];
            int[] dSwap;

            for (var i = 0; i <= maxi; i++) { dCurrent[i] = i; }

            int jm1 = 0, im1 = 0, im2 = -1;

            for (var j = 1; j <= maxj; j++)
            {

                // Rotate
                dSwap = dMinus2;
                dMinus2 = dMinus1;
                dMinus1 = dCurrent;
                dCurrent = dSwap;

                // Initialize
                var minDistance = Int32.MaxValue;
                dCurrent[0] = j;
                im1 = 0;
                im2 = -1;

                for (var i = 1; i <= maxi; i++)
                {

                    var cost = first[im1] == second[jm1] ? 0 : 1;

                    var del = dCurrent[im1] + 1;
                    var ins = dMinus1[i] + 1;
                    var sub = dMinus1[im1] + cost;

                    //Fastest execution for min value of 3 integers
                    var min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

                    if (i > 1 && j > 1 && first[im2] == second[jm1] && first[im1] == second[j - 2])
                        min = Math.Min(min, dMinus2[im2] + cost);

                    dCurrent[i] = min;
                    if (min < minDistance) { minDistance = min; }
                    im1++;
                    im2++;
                }
                jm1++;
                if (minDistance > threshold) { return new LevenshteinThresholdExceeded(minDistance); }
            }

            var result = dCurrent[maxi];

            if (result > threshold)
                return new LevenshteinThresholdExceeded(result);

            return new LevenshteinMatchingResult(result);
        }

        private static void Swap<T>(ref T arg1, ref T arg2)
        {
            T temp = arg1;
            arg1 = arg2;
            arg2 = temp;
        }
    }
}