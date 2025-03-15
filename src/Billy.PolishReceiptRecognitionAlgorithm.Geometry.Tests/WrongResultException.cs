using System;

namespace Billy.PolishReceiptRecognitionAlgorithm.Geometry.Tests
{
    public class WrongResultException : Exception
    {
        public WrongResultException(string message) : base(message)
        {
            
        }
    }
}