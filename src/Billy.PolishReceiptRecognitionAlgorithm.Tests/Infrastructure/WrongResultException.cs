using System;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests.Infrastructure
{
    public class WrongResultException : Exception
    {
        public WrongResultException(string message):base(message)
        {
            
        }
    }
}