using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Billy.CQRS;
using Billy.IdGeneration;
using Billy.Metrics;
using Billy.Receipts.Contracts;
using Billy.Receipts.Domain;
using Billy.Receipts.ReceiptRecognition;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;
using ReceiptsContract = Billy.Receipts.Contracts.Commands.Receipt.V1;

namespace Billy.Receipts.API.Commands
{
    public class RecognizeReceiptCommandHandler : 
        IHandler<ReceiptsContract.RecognizeReceipt, ReceiptsContract.RecognizeReceiptResponse>
    {
        private readonly ILogger _logger = Log.ForContext<RecognizeReceiptCommandHandler>();
        private readonly IReceiptStore _receiptStore;
        private readonly IBus _bus;
        private readonly IReceiptRecognitionProcessStateStore _receiptRecognitionProcessStateStore;
        private readonly IIdGenerator _idGenerator;
        private readonly UserReceiptsRecognitionPermissionsValidator _userReceiptsRecognitionPermissionsValidator;

        public RecognizeReceiptCommandHandler(
            IReceiptStore receiptStore,
            IBus bus,
            IReceiptRecognitionProcessStateStore receiptRecognitionProcessStateStore,
            IIdGenerator idGenerator,
            UserReceiptsRecognitionPermissionsValidator userReceiptsRecognitionPermissionsValidator)
        {
            _receiptStore = receiptStore;
            _bus = bus;
            _receiptRecognitionProcessStateStore = receiptRecognitionProcessStateStore;
            _idGenerator = idGenerator;
            _userReceiptsRecognitionPermissionsValidator = userReceiptsRecognitionPermissionsValidator;
        }

        public async Task<ReceiptsContract.RecognizeReceiptResponse> Handle(
            MessageContext<ReceiptsContract.RecognizeReceipt> context)
        {
            var receiptId = _idGenerator.NewId();

            using (LogContext.PushProperty("CorrelationId", context.CorrelationId))
            using (LogContext.PushProperty("GlobalUserIdentifier", context.Message.GlobalUserIdentifier))
            using (new LogicProcessingTimer(
                "recognize-receipt-command-handler", 
                "store-image-and-mark-recognition-as-started"))
            {
                //We accept the risk of over-processing more receipts.
                //Limit is updated after success operation. There is a risk of race conditions before limit will be updated.
                await EnsureThatUserHasPermissionsToRecognizeReceiptOrThrow(context);
                
                await using var imageStream = context.Message.Image.OpenReadStream();
                
                var (imageWidth, imageHeight) = GetImageDimensions(
                    context.Message, 
                    imageStream);
                
                var receiptImage = ReceiptImage.From(
                    imageStream,
                    context.Message.Image.GetFileExtension(),
                    imageWidth,
                    imageHeight);
                
                await StoreReceiptImage(receiptId, receiptImage);

                await MarkRecognitionAsStarted(receiptId, context.Message.GlobalUserIdentifier);

                await _bus.Publish(new Messages.V1.ReceiptAddedToPrintedTextRecognitionEvent()
                {
                    ReceiptId = receiptId,
                    CorrelationId = context.CorrelationId,
                    GlobalUserIdentifier = context.Message.GlobalUserIdentifier,
                    ImageHeight = receiptImage.ReceiptDimensions.ImageHeight,
                    ImageWidth = receiptImage.ReceiptDimensions.ImageWidth
                });
                
                return new ReceiptsContract.RecognizeReceiptResponse()
                {
                    ReceiptId = receiptId
                };
            }
        }

        private async Task StoreReceiptImage(
            Guid receiptId,
            ReceiptImage receiptImage)
        {
            using (new LogicProcessingTimer(
                "recognize-receipt-command-handler", 
                "store-receipt-image"))
            {
                await StoreReceipt(receiptImage, receiptId);
            }
        }

        private (int imageWidth, int imageHeight) GetImageDimensions(
            ReceiptsContract.RecognizeReceipt message, Stream imageStream)
        {
            if (!message.ImageHeight.HasValue ||
                !message.ImageWidth.HasValue)
            {
                var image = Image.FromStream(imageStream);
                return (image.Width, image.Height);
            }

            return (message.ImageWidth.Value, message.ImageHeight.Value);
        }

        private async Task MarkRecognitionAsStarted(Guid receiptId, string userId)
        {
            await _receiptRecognitionProcessStateStore.MarkRecognitionAsStarted(
                receiptId,
                userId);

            _logger.Information(
                "Receipt {receiptId} recognition process has been marked as started",
                receiptId);
        }

        private async Task EnsureThatUserHasPermissionsToRecognizeReceiptOrThrow(
            MessageContext<ReceiptsContract.RecognizeReceipt> context)
        {
            await _userReceiptsRecognitionPermissionsValidator
                .EnsureThatUserHasPermissionsToRecognizeReceiptOrThrow(
                    context.Message.GlobalUserIdentifier,
                    context.CorrelationId);
        }

        private async Task StoreReceipt(ReceiptImage receiptImage, Guid receiptId)
        {
            var receiptImageStorageId = await _receiptStore.StoreReceiptImage(
                receiptImage.ImageStream,
                receiptId);

            _logger.Information(
                "Receipt image has been stored on {storageName} with id {receiptImageStorageId}",
                _receiptStore.StorageName,
                receiptImageStorageId);
        }
    }
    
    public static class FormFileExtensions
    {
        public static string GetFileExtension(this IFormFile formFile) =>
            Path.GetExtension(formFile.FileName);
        
    }
}