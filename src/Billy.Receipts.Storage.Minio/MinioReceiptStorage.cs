using Billy.Metrics;
using Billy.Receipts.Domain;
using Minio.Exceptions;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Billy.Receipts.Storage.Minio
{
    public class MinioReceiptStorage : IReceiptStore
    {
        private static readonly ILogger Logger = Log.ForContext<MinioReceiptStorage>();

        private readonly IMinioStorage _minioStorage;

        public MinioReceiptStorage(IMinioStorage minioStorage)
        {
            _minioStorage = minioStorage;

        }

        public StorageName StorageName { get; } = StorageName.From("min.io Storage");

        public async Task<string> StoreReceiptImage(Stream fileStream, Guid receiptId)
        {
            using (new StorageAccessTimer("store_receipt_image", StorageName.Value))
            {
                var receiptImageStoragePath = MinioStoragePathForReceiptRecognition.ForReceiptImage(receiptId);

                await Store(fileStream, receiptImageStoragePath);

                return receiptImageStoragePath;
            }
        }

        public async Task<string> StorePrintedTextRecognitionResult<TPrintedTextRecognition>(
            TPrintedTextRecognition model, Guid receiptId)
            where TPrintedTextRecognition: class
        {
            using (new StorageAccessTimer("store_printed_text_recognition_result", StorageName.Value))
            {
                var storagePath = MinioStoragePathForReceiptRecognition.ForPrintedTextRecognitionResult(receiptId);

                await Store(model, storagePath);

                return storagePath;
            }
        }

        public async Task<Stream> GetReceiptImageStream(Guid receiptId)
        {
            using (new StorageAccessTimer("get_receipt_image", StorageName.Value))
            {
                var receiptImageStoragePath = MinioStoragePathForReceiptRecognition.ForReceiptImage(receiptId);
                return await _minioStorage.ReadStreamFile(receiptImageStoragePath);
            }
        }
        
        public async Task<string> StoreRecognizedReceipt<TReceiptModel>(TReceiptModel receipt, Guid receiptId)
            where TReceiptModel : class
        {
            using (new StorageAccessTimer("store_recognized_receipt", StorageName.Value))
            {
                var recognizedReceiptStoragePath = MinioStoragePathForReceiptRecognition.ForRecognizedReceipt(receiptId);

                await Store(receipt, recognizedReceiptStoragePath);

                return recognizedReceiptStoragePath;
            }
        }


        private async Task Store(Stream receiptFileStream, string fullPath)
        {
            try
            {
                Logger.Debug("Starting uploading receipt file to {endpoint}/{bucketName}/{fullPath}",
                    _minioStorage.Endpoint, _minioStorage.BucketName, fullPath);

                await _minioStorage.Save(receiptFileStream, fullPath);

                Logger.Debug("Uploaded receipt file to {endpoint}/{bucketName}/{fullPath}",
                    _minioStorage.Endpoint, _minioStorage.BucketName, fullPath);
            }
            catch (MinioException minioException)
            {
                Logger.Error(
                    minioException, 
                    "Receipt file uploading fail. Destination: {endpoint}/{bucketName}/{fullPath}. Minio Response: {@Response}",  
                    _minioStorage.Endpoint, 
                    _minioStorage.BucketName, 
                    fullPath,
                    minioException.Response);
                throw;
            }
        }

        private async Task Store<TModel>(TModel receipt, string fullPath)
        {
            var receiptJson = JsonConvert.SerializeObject(receipt);

            await Store(Encoding.ASCII.GetBytes(receiptJson), fullPath);
        }



    }

    public class MinioStoragePathForReceiptRecognition
    {
        public static string ForReceiptImage(Guid receiptId) =>
            $"{receiptId}/ReceiptImage";

        public static string ForPrintedTextRecognitionResult(Guid receiptId) =>
            $"{receiptId}/PrintedTextRecognitionResult.json";

        public static string ForRawRecognitionAlgorithmResult(Guid receiptId) =>
            $"{receiptId}/RecognitionAlgorithmResult.json";

        public static string ForRecognizedReceipt(Guid receiptId) =>
            $"{receiptId}/RecognizedReceipt.json";
    }
}
