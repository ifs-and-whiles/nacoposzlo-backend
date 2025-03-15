using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Billy.CodeReadability;

namespace Billy.Receipts.Domain
{
    public interface IReceiptParser
    {
        Either<ReceiptParsed, ReceiptNotParsed> Parse(
            PrintedTextRecognitionResult printedTextRecognitionResult,
            ReceiptDimensions receiptDimensions);
    }

    public class ReceiptNotParsed
    {
        public AlgorithmName AlgorithmName { get; }

        public ReceiptNotParsed(AlgorithmName algorithmName)
        {
            AlgorithmName = algorithmName;
        }
    }

    public class ReceiptParsed
    {
        public RecognizedReceipt RecognizedReceipt { get; }
        public AlgorithmName AlgorithmName { get;  }
        public RawAlgorithmResult RawAlgorithmResult { get; }

        public ReceiptParsed(
            RecognizedReceipt recognizedReceipt, 
            AlgorithmName algorithmName, 
            RawAlgorithmResult rawAlgorithmResult)
        {
            RecognizedReceipt = recognizedReceipt;
            AlgorithmName = algorithmName;
            RawAlgorithmResult = rawAlgorithmResult;
        }
    }
}
