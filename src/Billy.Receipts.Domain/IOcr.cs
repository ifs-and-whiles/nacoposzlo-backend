using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Billy.Receipts.Domain
{
    public interface IOcr
    {
        OcrProviderName ProviderName { get; }
        Task<PrintedTextRecognitionResult> RecognizeImage(
            ReceiptId receiptId,
            ReceiptDimensions receiptDimensions);
    }
}
