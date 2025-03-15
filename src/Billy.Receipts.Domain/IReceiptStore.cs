using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Billy.Receipts.Domain
{
    public interface IReceiptStore
    {
        StorageName StorageName { get; }

        Task<string> StoreReceiptImage(Stream fileStream, Guid receiptId);

        Task<string> StorePrintedTextRecognitionResult<TPrintedTextRecognition>(TPrintedTextRecognition model, Guid receiptId)
            where TPrintedTextRecognition : class;
        
        Task<Stream> GetReceiptImageStream(Guid receiptId);


        Task<string> StoreRecognizedReceipt<TReceiptModel>(TReceiptModel receipt, Guid receiptId)
            where TReceiptModel : class;

    }

    public class KeyAlreadyExistsOnStorageException : Exception
    {
        public string KeyValue { get; }

        public KeyAlreadyExistsOnStorageException(string keyValue)
        {
            KeyValue = keyValue;
        }
    }

    public class KeyNotFoundOnStorageException : Exception
    {
        public string KeyValue { get; }
        public KeyNotFoundOnStorageException(string keyValue)
        {
            KeyValue = keyValue;
        }
    }
}
