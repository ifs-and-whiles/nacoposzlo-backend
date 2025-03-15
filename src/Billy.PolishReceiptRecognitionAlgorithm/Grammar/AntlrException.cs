using System;
using System.Collections.Generic;

namespace Billy.PolishReceiptRecognitionAlgorithm.Grammar
{
    public class AntlrException : AggregateException
    {
        public AntlrException(string message, IEnumerable<Exception> innerExceptions) : base(message, innerExceptions)
        {
        }
    }
}
