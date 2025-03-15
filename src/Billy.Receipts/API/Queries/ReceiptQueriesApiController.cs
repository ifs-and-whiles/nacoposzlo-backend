using System.Threading.Tasks;
using Billy.CQRS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Requests = Billy.Receipts.Contracts.Queries.Receipts.V1;
using ReadModel = Billy.Receipts.Contracts.Queries.Receipts.V1.ReadModels;

namespace Billy.Receipts.API.Queries
{
    [Authorize(Policy = "ServiceUsers")]
    [ApiController, Route(ReceiptsApiRouteConsts.Prefix + "/v1/receipts/queries")]
    public class ReceiptQueriesApiController: ReceiptApiControllerBase
    {
        private readonly IHandler<Requests.GetReceiptRecognitionProcessStatus, ReadModel.ReceiptRecognitionProcessState> 
            _getReceiptRecognitionProcessStatusQueryHandler;

        private readonly IHandler<Requests.GetRecognizedReceipt, ReadModel.Receipt> 
            _getRecognizedReceipt;

        private readonly IHandler<Requests.GetReceiptImage, ReadModel.ReceiptImage> _getReceiptImageQueryHandler;

        public ReceiptQueriesApiController(
            IHandler<Requests.GetReceiptRecognitionProcessStatus, ReadModel.ReceiptRecognitionProcessState> 
                getReceiptRecognitionProcessStatusQueryHandler,
            IHandler<Requests.GetRecognizedReceipt, ReadModel.Receipt> 
                getRecognizedReceipt,
            IHandler<Requests.GetReceiptImage, ReadModel.ReceiptImage> getReceiptImageQueryHandler)
        {
            _getReceiptRecognitionProcessStatusQueryHandler = getReceiptRecognitionProcessStatusQueryHandler;
            _getRecognizedReceipt = getRecognizedReceipt;
            _getReceiptImageQueryHandler = getReceiptImageQueryHandler;
        }

        [HttpGet, Route("get-recognized-receipt")]
        public async Task<ReadModel.Receipt> GetRecognizedReceipt(
            [FromQuery] Requests.GetRecognizedReceipt query)
        {
            return await _getRecognizedReceipt
                .Handle(new MessageContext<Requests.GetRecognizedReceipt>
                {
                    Message = query,
                    CorrelationId = GetRequestCorrelationId()
                });
        }

        [HttpGet, Route("get-receipt-recognition-process-status")]
        public async Task<ReadModel.ReceiptRecognitionProcessState> GetReceiptRecognitionProcessStatus(
            [FromQuery] Requests.GetReceiptRecognitionProcessStatus query)
        {
            return await _getReceiptRecognitionProcessStatusQueryHandler
                .Handle(new MessageContext<Requests.GetReceiptRecognitionProcessStatus>
                {
                    Message = query,
                    CorrelationId = GetRequestCorrelationId()
                });
        }

        [HttpGet, Route("get-receipt-image")]
        public async Task<ReadModel.ReceiptImage> GetReceiptImage(
            [FromQuery] Requests.GetReceiptImage query)
        {
            return await _getReceiptImageQueryHandler.Handle(new MessageContext<Requests.GetReceiptImage>()
            {
                Message = query,
                CorrelationId = GetRequestCorrelationId()
            });
        }
    }
}
