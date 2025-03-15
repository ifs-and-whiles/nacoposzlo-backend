using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Billy.Api.Mobile.ExpensesService;
using Billy.Api.Mobile.Utils;
using Billy.Receipts.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceModel = Billy.Receipts.Contracts.Queries.Receipts.V1;
using MobileApiModel = Billy.Api.Mobile.API.Contract.Mobile.V1.Receipts;

namespace Billy.Api.Mobile.API
{
    [Authorize]
    [ApiController, Route("mobile-api/v1/receipts")]
    public class ReceiptMobileApiController : MobileApiControllerBase
    {
        private readonly ReceiptsServiceClient _receiptsServiceClient;

        public ReceiptMobileApiController(ReceiptsServiceClient receiptsServiceClient)
        {
            _receiptsServiceClient = receiptsServiceClient;
        }

        [HttpGet, Route("get-recognized-receipt")]
        public async Task<MobileApiModel.Receipt> GetRecognizedReceipt(
            [FromQuery]MobileApiModel.GetRecognizedReceiptRequest request)
        {
            var recognizedReceipt = await _receiptsServiceClient.GetRecognizedReceipt(new ServiceModel.GetRecognizedReceipt()
            {
                ReceiptId = request.ReceiptId,
                CorrelationId = GetRequestCorrelationId()
            });
            
            return new MobileApiModel.Receipt()
            {
                Amount = recognizedReceipt.Amount,
                Date = recognizedReceipt.Date,
                Id = recognizedReceipt.Id,
                Seller = recognizedReceipt.Seller,
                Products = recognizedReceipt.Products.Select(p => new MobileApiModel.Product()
                {
                    IsRecognized = p.IsRecognized,
                    Amount = p.Amount,
                    IsCorrupted = p.IsCorrupted,
                    Name = p.Name,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice
                }).ToList(),
                Problems = recognizedReceipt.Problems.Select(p => p switch
                {
                    (ServiceModel.ReadModels.Problem.AmountDifferentThanSumOfProducts) => MobileApiModel.Problem.AmountDifferentThanSumOfProducts,
                    _ => throw new ContractMappingException(message: $"Receipt recognition problem {Enum.GetName(typeof(MobileApiModel.Problem), p)} does not exist in mobile api contract"),
                }).ToList()
            };
        }

        [HttpGet, Route("get-receipt-recognition-process-status")]
        public async Task<MobileApiModel.ReceiptRecognitionProcessState> GetReceiptRecognitionProcessStatus(
            [FromQuery]MobileApiModel.GetReceiptRecognitionProcessStatusRequest request)
        {
            var recognitionState = await _receiptsServiceClient.GetReceiptRecognitionProcessStatus(
                new ServiceModel.GetReceiptRecognitionProcessStatus()
                {
                    ReceiptId = request.ReceiptId,
                    CorrelationId = GetRequestCorrelationId()
                });

            return new MobileApiModel.ReceiptRecognitionProcessState()
            {
                Status = recognitionState.Status switch
                {
                    (ServiceModel.ReadModels.ReceiptRecognitionProcessStatus.RecognitionStarted) => MobileApiModel.ReceiptRecognitionProcessStatus.RecognitionStarted,
                    (ServiceModel.ReadModels.ReceiptRecognitionProcessStatus.RecognitionCompleted) => MobileApiModel.ReceiptRecognitionProcessStatus.RecognitionCompleted,
                    (ServiceModel.ReadModels.ReceiptRecognitionProcessStatus.RecognitionFailed) => MobileApiModel.ReceiptRecognitionProcessStatus.RecognitionFailed,

                    _ => throw new ContractMappingException(message: 
                        $"Receipt recognition status {Enum.GetName(typeof(ServiceModel.ReadModels.ReceiptRecognitionProcessStatus), recognitionState.Status)} does not exist in mobile api contract"),
                }
            };
        }

        [HttpPost, Route("recognize")]
        public async Task<Guid> RecognizeReceipt(
            [FromBody]MobileApiModel.RecognizeReceiptRequest request)
        {
            var receiptId = Guid.NewGuid();
            await _receiptsServiceClient.RecognizeReceipt(new Commands.Receipt.V1.RecognizeReceipt()
            {
                ImageInBase64 = request.ImageInBase64,
                ImageExtension = request.ImageExtension,
                ReceiptId = receiptId,
                CorrelationId = GetRequestCorrelationId()
            });
            return receiptId;
        }
    }

    public class ContractMappingException : Exception
    {
        public ContractMappingException(string message): base(message)
        {
            
        }
    }
}
