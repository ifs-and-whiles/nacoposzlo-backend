using Autofac;
using Billy.CQRS;
using Billy.Receipts.API.Commands;
using Billy.Receipts.API.Queries;
using Billy.Receipts.AWS.VisionAPI;
using Billy.Receipts.Domain;
using Billy.Receipts.Infrastructure.Configs;
using Billy.Receipts.ReceiptRecognition;
using Billy.Receipts.Recognition.PolishReceiptRecognitionAlgorithm;
using Billy.Receipts.Storage.Minio;
using Billy.Receipts.Storage.S3;
using Billy.Receipts.UsersService;
using Requests = Billy.Receipts.Contracts.Queries.Receipts.V1;
using ReadModel = Billy.Receipts.Contracts.Queries.Receipts.V1.ReadModels;

namespace Billy.Receipts
{
    public class ReceiptsModule : Module
    {
        private readonly StorageConfig _storageConfig;
        private readonly OCRConfig _ocrConfig;

        public ReceiptsModule(StorageConfig storageConfig, OCRConfig ocrConfig)
        {
            _storageConfig = storageConfig;
            _ocrConfig = ocrConfig;
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            //STORAGE
            if (_storageConfig.StorageProvider == StorageProvider.Minio)
            {
                builder
                    .RegisterType<MinioStorage>()
                    .As<IMinioStorage>();
                
                builder
                    .RegisterType<MinioReceiptStorage>()
                    .As<IReceiptStore>();
            }

            if (_storageConfig.StorageProvider == StorageProvider.S3)
                builder
                    .RegisterType<S3ReceiptStorage>()
                    .As<IReceiptStore>();
            
            
            //OCR

            if (_ocrConfig.OCRProvider == OCRProvider.AWS)
            {
                builder
                    .RegisterType<AWSOcrProcessor>()
                    .As<IOcr>();
            }

            
            
            builder
                .RegisterType<UsersServiceClient>();

            builder
                .RegisterType<UserReceiptsRecognitionPermissionsValidator>();

            builder
                .RegisterType<PolishReceiptParser>()
                .As<IReceiptParser>();

            builder
                .RegisterType<MartenReceiptRecognitionProcessStateStore>()
                .As<IReceiptRecognitionProcessStateStore>();

            //Message handlers
            builder
                .RegisterType<RecognizeReceiptCommandHandler>()
                .As<IHandler<Contracts.Commands.Receipt.V1.RecognizeReceipt, Contracts.Commands.Receipt.V1.RecognizeReceiptResponse>>();

            //Query handlers
            builder
                .RegisterType<GetRecognizedReceiptQueryHandler>()
                .As<IHandler<Requests.GetRecognizedReceipt, ReadModel.Receipt>>();
            
            builder
                .RegisterType<GetReceiptRecognitionProcessStatusQueryHandler>()
                .As<IHandler<Requests.GetReceiptRecognitionProcessStatus, ReadModel.ReceiptRecognitionProcessState>>();

            builder
                .RegisterType<GetReceiptImageQueryHandler>()
                .As<IHandler<Requests.GetReceiptImage, ReadModel.ReceiptImage>>();


        }
    }
}
