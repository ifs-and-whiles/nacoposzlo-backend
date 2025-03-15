using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Billy.Receipts.Domain
{
    public interface IReceiptRecognitionProcessStateStore
    {
        Task MarkRecognitionAsStarted(Guid receiptId, string userId);

        Task MarkRecognitionAsFailed(Guid receiptId);
    }
}
