using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using Billy.Metrics;
using Billy.Receipts.Domain;
using Newtonsoft.Json;
using Serilog;

namespace Billy.Receipts.Storage.S3
{
    public class S3ReceiptStorage : IReceiptStore
    {
        private readonly IAmazonS3 _amazonS3;
        private readonly S3StorageConfig _storageConfig;
        public StorageName StorageName { get; } = StorageName.From("AWS S3 Storage");
        
        private static readonly ILogger Logger = Log.ForContext<S3ReceiptStorage>();
        
        public S3ReceiptStorage(IAmazonS3 amazonS3, S3StorageConfig storageConfig)
        {
            _amazonS3 = amazonS3;
            _storageConfig = storageConfig;
        }
        
        public async Task<string> StoreReceiptImage(Stream fileStream, Guid receiptId)
        {
            using (new StorageAccessTimer("store_receipt_image", StorageName.Value))
            {
                var receiptImageStoragePath = S3StoragePathForReceiptRecognition.ForReceiptImage(receiptId);
                
                try
                {
                    var fileTransferUtility = new TransferUtility(_amazonS3);

                    await fileTransferUtility.UploadAsync(fileStream, _storageConfig.BucketName, receiptImageStoragePath);
    
                    return receiptImageStoragePath;
                }
                catch (AmazonS3Exception e)
                {
                    Logger.Error(e, "Error while uploading receipt image ReceiptId {ReceiptId} to {S3Path}",
                        receiptId,
                        receiptImageStoragePath);
                    throw;
                }
            }
        }

        public async Task<string> StorePrintedTextRecognitionResult<TPrintedTextRecognitionResult>(
            TPrintedTextRecognitionResult model, Guid receiptId)
            where TPrintedTextRecognitionResult: class
        {
            using (new StorageAccessTimer("store_printed_text_recognition_result", StorageName.Value))
            {
                var storagePath = S3StoragePathForReceiptRecognition.ForPrintedTextRecognitionResult(receiptId);
                
                try
                {
                    await StoreObject(model, storagePath);

                    return storagePath;
                }
                catch (AmazonS3Exception e)
                {
                    Logger.Error(e, "Error while uploading Printed Text Recognition Result. " +
                                    "ReceiptId {ReceiptId} to {S3Path}",
                        receiptId,
                        storagePath);
                    //TODO:[FP] throw exception
                    throw;
                }
            }
        }

        public async Task<Stream> GetReceiptImageStream(Guid receiptId)
        {
            using (new StorageAccessTimer("get_receipt_image", StorageName.Value))
            {
                var receiptImageStoragePath = S3StoragePathForReceiptRecognition.ForReceiptImage(receiptId);
                try
                {
                    var fileTransferUtility = new TransferUtility(_amazonS3);
                    return await fileTransferUtility.OpenStreamAsync(_storageConfig.BucketName, receiptImageStoragePath);
                }
                catch (AmazonS3Exception e)
                {
                    Logger.Error(e, "Error while downloading Receipt Image. " +
                                    "ReceiptId {ReceiptId} to {S3Path}",
                        receiptId,
                        receiptImageStoragePath);
                    throw;
                }
            }
        }
        
        public async Task<string> StoreRecognizedReceipt<TReceiptModel>(TReceiptModel receipt, Guid receiptId) 
            where TReceiptModel: class
        {
            using (new StorageAccessTimer("store_recognized_receipt", StorageName.Value))
            {
                var recognizedReceiptStoragePath = S3StoragePathForReceiptRecognition.ForRecognizedReceipt(receiptId);
                
                try
                {
                    await StoreObject(receipt, recognizedReceiptStoragePath);

                    return recognizedReceiptStoragePath;
                }
                catch (AmazonS3Exception e)
                {
                    Logger.Error(e, "Error while uploading Recognized Receipt. " +
                                    "ReceiptId {ReceiptId} to {S3Path}",
                        receiptId,
                        recognizedReceiptStoragePath);
                    //TODO:[FP] throw exception
                    throw;
                }
            }
        }

        private async Task StoreObject<TObject>(TObject unifiedOcrResult, string path) where TObject : class
        {
            var fileTransferUtility = new TransferUtility(_amazonS3);

            await using var memoryStream = new MemoryStream();
            
            var unifiedOcrResultJson = JsonConvert.SerializeObject(unifiedOcrResult);

            var unifiedOcrResultInBytes = Encoding.ASCII.GetBytes(unifiedOcrResultJson);

            await memoryStream.WriteAsync(unifiedOcrResultInBytes, 0, unifiedOcrResultInBytes.Length);

            await fileTransferUtility.UploadAsync(memoryStream, _storageConfig.BucketName, path);
        }
    }
    
    public class S3StoragePathForReceiptRecognition
    {
        public static string ForReceiptImage(Guid receiptId) =>
            $"{receiptId}/ReceiptImage";

        public static string ForPrintedTextRecognitionResult(Guid receiptId) =>
            $"{receiptId}/PrintedTextRecognitionResult";

        public static string ForRecognizedReceipt(Guid receiptId) =>
            $"{receiptId}/RecognizedReceipt";
    }
}