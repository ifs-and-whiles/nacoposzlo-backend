using System;
using System.Collections.Generic;
using System.Text;

namespace Billy.PolishReceiptRecognitionAlgorithm.OcrJson
{
    public class GoldenSectionSearch
    {
        private readonly double _goldenRatio = 2.0 - (1.0 + Math.Sqrt(5.0)) / 2.0;
        private readonly double _threshold;
        private readonly int _maxIterations;

        public GoldenSectionSearch(
            double threshold,
            int maxIterations)
        {
            _threshold = threshold;
            _maxIterations = maxIterations;
        }

        public double Minimize(Func<double, double> function, double lowerBound, double upperBound)
        {
            var f = CachedFunction(
                function);

            var midpoint = StartingSearchPoint(
                lowerBound, 
                upperBound);

            return Minimize(
                f, 
                lowerBound, 
                midpoint, 
                upperBound);
        }

        private double Minimize(
            Func<double, double> function, 
            double lower, 
            double midpoint, 
            double upper)
        {
            var iterations = 0;
            var currentLower = lower;
            var currentMid = midpoint;
            var currentUpper = upper;
            double currentX;

            do
            {
                iterations++;

                currentX = NextSearchPoint(
                    currentLower, 
                    currentMid, 
                    currentUpper);
                
                if (IsMinimumInNewSearchBracket(function, currentMid, currentX))
                {
                    if (IsUpperBracketLarger(currentLower, currentMid, currentUpper))
                    {
                        currentLower = currentMid;
                        currentMid = currentX;
                    }
                    else
                    {
                        currentMid = currentX;
                    }

                }
                else
                {
                    if (IsUpperBracketLarger(currentLower, currentMid, currentUpper))
                    {
                        currentUpper = currentX;
                    }
                    else
                    {
                        currentLower = currentX;
                    }
                }
            } while (!IsSearchDone(currentLower, currentMid, currentUpper, currentX) && iterations < _maxIterations);

            return FinalPosition(currentLower, currentUpper);
        }

        private Func<double, double> CachedFunction(Func<double, double> function)
        {
            var cache = new Dictionary<double, double>();
            
            return (x) =>
            {
                if (cache.ContainsKey(x))
                {
                    return cache[x];
                }
                var result = function(x);
                cache[x] = result;
                return result;
            };
        }

        private double StartingSearchPoint(double lower, double upper)
        {
            return (upper - lower) * _goldenRatio + lower;
        }

        private double NextSearchPoint(double lower, double midpoint, double upper)
        {
            if (IsUpperBracketLarger(lower, midpoint, upper))
                return midpoint + _goldenRatio * (upper - midpoint);
            return midpoint - _goldenRatio * (midpoint - lower);
        }

        private bool IsSearchDone(double lower, double midpoint, double upper, double x)
        {
            return Math.Abs(upper - lower) < _threshold * (Math.Abs(midpoint) + Math.Abs(x));
        }

        private double FinalPosition(double lower, double upper)
        {
            return (upper + lower) / 2.0;
        }

        private bool IsUpperBracketLarger(double lower, double midpoint, double upper)
        {
            return upper - midpoint > midpoint - lower;
        }

        private bool IsMinimumInNewSearchBracket(Func<double, double> function, double midpoint, double x)
        {
            return function(x) < function(midpoint);
        }
    }
}
