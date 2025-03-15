using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Billy.CQRS;
using Billy.MobileApi.Common;
using Billy.MobileApi.Contracts.Receipts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileApiReceiptsContract = Billy.MobileApi.Contracts.Receipts.Contract.Mobile.V1.Receipts;
using Commands = Billy.Receipts.Contracts.Commands.Receipt.V1;
using Queries = Billy.Receipts.Contracts.Queries.Receipts.V1;

namespace Billy.MobileApi.API
{
    [Authorize(Policy = "ExternalApiUsers")]
    [ApiController, Route(MobileApiRouteConsts.Prefix + "/v1/receipts")]
    public class ReceiptMobileApiController : MobileApiControllerBase
    {
        private readonly IHandler<Commands.RecognizeReceipt, Commands.RecognizeReceiptResponse> 
            _recognizeReceiptHandler;

        private readonly IHandler<Queries.GetRecognizedReceipt, Queries.ReadModels.Receipt> 
            _getRecognizedReceiptQueryHandler;

        private readonly IHandler<Queries.GetReceiptRecognitionProcessStatus, Queries.ReadModels.ReceiptRecognitionProcessState> 
            _getReceiptRecognitionProcessStatusQueryHandler;

        private readonly IHandler<Queries.GetReceiptImage, Queries.ReadModels.ReceiptImage> _getReceiptImageQueryHandler;

        public ReceiptMobileApiController(
            IHandler<Commands.RecognizeReceipt, Commands.RecognizeReceiptResponse>
                recognizeReceiptHandler,
            IHandler<Queries.GetRecognizedReceipt, Queries.ReadModels.Receipt>
                getRecognizedReceiptQueryHandler,
            IHandler<Queries.GetReceiptRecognitionProcessStatus, Queries.ReadModels.ReceiptRecognitionProcessState>
                getReceiptRecognitionProcessStatusQueryHandler,
            IHandler<Queries.GetReceiptImage, Queries.ReadModels.ReceiptImage> 
                getReceiptImageQueryHandler)
        {
            _recognizeReceiptHandler = recognizeReceiptHandler;
            _getRecognizedReceiptQueryHandler = getRecognizedReceiptQueryHandler;
            _getReceiptRecognitionProcessStatusQueryHandler = getReceiptRecognitionProcessStatusQueryHandler;
            _getReceiptImageQueryHandler = getReceiptImageQueryHandler;
        }

        [HttpGet, Route("get-receipt-image")]
        public async Task<IActionResult> GetReceiptImage(
            [FromQuery]MobileApiReceiptsContract.GetReceiptImageRequest request)
        {
            var image = await _getReceiptImageQueryHandler.Handle(new MessageContext<Queries.GetReceiptImage>()
            {
                Message = new Queries.GetReceiptImage()
                {
                    ReceiptId = request.ReceiptId
                },
                CorrelationId = GetRequestCorrelationId()
            });
            
            return new FileStreamResult(image.Image, image.ContentType);
        }

        [HttpPost, Route("recognize")]
        public async Task<Guid> RecognizeReceipt(
            [FromForm] MobileApiReceiptsContract.RecognizeReceiptRequest request)
        {
            var result = await _recognizeReceiptHandler.Handle(new MessageContext<Commands.RecognizeReceipt>
            {
                Message = new Commands.RecognizeReceipt
                {
                    Image = request.Image,
                    ImageHeight = request.ImageHeight,
                    ImageWidth = request.ImageWidth,
                    GlobalUserIdentifier = GetUserId(),
                },
                CorrelationId = GetRequestCorrelationId()
            });

            return result.ReceiptId;
        }
            

        [HttpGet, Route("get-recognized-receipt")]
        public async Task<MobileApiReceiptsContract.Receipt> GetRecognizedReceipt(
            [FromQuery] MobileApiReceiptsContract.GetRecognizedReceiptRequest request)
        {
            var recognizedReceipt = await _getRecognizedReceiptQueryHandler
                .Handle(new MessageContext<Queries.GetRecognizedReceipt>
                {
                    Message = new Queries.GetRecognizedReceipt
                    {
                        ReceiptId = request.ReceiptId
                    },
                    CorrelationId = GetRequestCorrelationId()
                });

            return new MobileApiReceiptsContract.Receipt()
            {
                Amount = recognizedReceipt.Amount,
                Date = recognizedReceipt.Date,
                Id = recognizedReceipt.Id,
                Seller = recognizedReceipt.Seller,
                OriginalOrientation = MapOriginalOrientation(recognizedReceipt),
                Products = MapProducts(recognizedReceipt),
                Problems = MapProblems(recognizedReceipt)
            };
        }

        private static MobileApiReceiptsContract.Orientation MapOriginalOrientation(
            Queries.ReadModels.Receipt recognizedReceipt)
        {
            return recognizedReceipt.OriginalOrientation == null
                ? null
                : new MobileApiReceiptsContract.Orientation
                {
                    ValueInRadians = recognizedReceipt.OriginalOrientation.ValueInRadians
                };
        }

        private static List<MobileApiReceiptsContract.Product> MapProducts(Queries.ReadModels.Receipt recognizedReceipt)
        {
            return recognizedReceipt.Products.Select(p => new MobileApiReceiptsContract.Product()
            {
                IsRecognized = p.IsRecognized,
                Amount = p.Amount,
                IsCorrupted = p.IsCorrupted,
                Name = p.Name,
                Quantity = p.Quantity,
                UnitPrice = p.UnitPrice,
                BoundingBox = MapReadModelToMobileBoundingBox(p.BoundingBox)
            }).ToList();
        }

        private static Contracts.Receipts.Contract.Mobile.V1.Receipts.BoundingBox MapReadModelToMobileBoundingBox(
            Billy.Receipts.Contracts.Queries.Receipts.V1.ReadModels.BoundingBox boundingBox)
        {
            return new MobileApiReceiptsContract.BoundingBox
            {
                LeftBottom = MapReadModelToMobilePoint(boundingBox.LeftBottom),
                RightTop = MapReadModelToMobilePoint(boundingBox.RightTop),
                LeftTop = MapReadModelToMobilePoint(boundingBox.LeftTop),
                RightBottom = MapReadModelToMobilePoint(boundingBox.RightBottom)
            };
        }

        private static Contracts.Receipts.Contract.Mobile.V1.Receipts.Point MapReadModelToMobilePoint(
            Billy.Receipts.Contracts.Queries.Receipts.V1.ReadModels.Point point)
        {
            return new MobileApiReceiptsContract.Point
            {
                X = point.X,
                Y = point.Y
            };
        }

        private static List<MobileApiReceiptsContract.Problem> MapProblems(Queries.ReadModels.Receipt recognizedReceipt)
        {
            return recognizedReceipt.Problems.Select(p => p switch
            {
                (Queries.ReadModels.Problem.AmountDifferentThanSumOfProducts) => MobileApiReceiptsContract.Problem.AmountDifferentThanSumOfProducts,
                _ => throw new ContractMappingException(message:
                    $"Receipt recognition problem {Enum.GetName(typeof(MobileApiReceiptsContract.Problem), p)} does not exist in mobile api contract"),
            }).ToList();
        }

        [HttpGet, Route("get-receipt-recognition-process-status")]
        public async Task<MobileApiReceiptsContract.ReceiptRecognitionProcessState> GetReceiptRecognitionProcessStatus(
            [FromQuery] MobileApiReceiptsContract.GetReceiptRecognitionProcessStatusRequest request)
        {
            var recognitionState = await _getReceiptRecognitionProcessStatusQueryHandler
                .Handle(new MessageContext<Queries.GetReceiptRecognitionProcessStatus>
                {
                    Message = new Queries.GetReceiptRecognitionProcessStatus
                    {
                        ReceiptId = request.ReceiptId
                    },
                    CorrelationId = GetRequestCorrelationId()
                });
            
            return new MobileApiReceiptsContract.ReceiptRecognitionProcessState()
            {
                Status = recognitionState.Status switch
                {
                    (Queries.ReadModels.ReceiptRecognitionProcessStatus.RecognitionStarted) => 
                    MobileApiReceiptsContract.ReceiptRecognitionProcessStatus.RecognitionStarted,
                    
                    (Queries.ReadModels.ReceiptRecognitionProcessStatus.RecognitionCompleted) => 
                    MobileApiReceiptsContract.ReceiptRecognitionProcessStatus.RecognitionCompleted,
                    
                    (Queries.ReadModels.ReceiptRecognitionProcessStatus.RecognitionFailed) => 
                    MobileApiReceiptsContract.ReceiptRecognitionProcessStatus.RecognitionFailed,

                    _ => throw new ContractMappingException(message: 
                        $"Receipt recognition status {Enum.GetName(typeof(Queries.ReadModels.ReceiptRecognitionProcessStatus), recognitionState.Status)} " +
                        $"does not exist in mobile api contract"),
                }
            };
        }
    }
}