using System.Threading.Tasks;
using Billy.CQRS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReceiptContract = Billy.Receipts.Contracts.Commands.Receipt.V1;
namespace Billy.Receipts.API.Commands
{
    [Authorize(Policy = "ServiceUsers")]
    [ApiController, Route(ReceiptsApiRouteConsts.Prefix + "/v1/receipts/commands")]
    public class ReceiptCommandApiController : ReceiptApiControllerBase
    {
        private readonly IHandler<ReceiptContract.RecognizeReceipt, Contracts.Commands.Receipt.V1.RecognizeReceiptResponse> _recognizeReceiptHandler;

        public ReceiptCommandApiController(IHandler<ReceiptContract.RecognizeReceipt, ReceiptContract.RecognizeReceiptResponse>
            recognizeReceiptHandler)
        {
            _recognizeReceiptHandler = recognizeReceiptHandler;
        }

        [HttpPost, Route("recognize")]
        public async Task<ReceiptContract.RecognizeReceiptResponse> RecognizeReceipt(
            [FromForm] ReceiptContract.RecognizeReceipt command)
        {
            return await _recognizeReceiptHandler.Handle(new MessageContext<ReceiptContract.RecognizeReceipt>()
            {
                Message = command,
                CorrelationId = GetRequestCorrelationId()
            });
        }
    }
}
