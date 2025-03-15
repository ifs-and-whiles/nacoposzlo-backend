using System;
using System.Collections.Generic;
using System.Text;

namespace Billy.Receipts.Domain
{
    public class ErrorCodes
    {
        public static string AlgorithmNameCanNotBeEmpty = "AlgorithmNameCanNotBeEmpty";
        public static string ReceiptImageDoesNotExistOnStorage = "ReceiptImageDoesNotExistOnStorage";
        public static string ReceiptInterpretationByAlgorithmFailed = "ReceiptInterpretationByAlgorithmFailed";
        public static string ReceiptImageCanNotBeEmpty = "ReceiptImageCanNotBeEmpty";
        public static string ReceiptDimensionsCanNotBeEmpty = "ReceiptDimensionsCanNotBeEmpty";
        public static string IncorrectReceiptImageExtension = "IncorrectReceiptImageExtension";
        public static string IncorrectImageBase64Format = "IncorrectImageBase64Format";
    }
}
