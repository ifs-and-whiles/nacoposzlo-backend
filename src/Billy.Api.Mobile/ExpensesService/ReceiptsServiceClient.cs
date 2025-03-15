using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Billy.Api.Mobile.Infrastructure.Configs;
using Billy.Api.Mobile.Utils;
using Billy.Receipts.Contracts;
using Flurl.Http.Configuration;

namespace Billy.Api.Mobile.ExpensesService
{
    public class ReceiptsServiceClient
    {
        private readonly HttpServiceClient _httpServiceClient;
        
        public ReceiptsServiceClient(
            IFlurlClientFactory flurlClientFactory,
            ReceiptsServiceConfig receiptsServiceConfig,
            RequestRetryPolicyConfig requestRetryPolicyConfig)
        {
            _httpServiceClient = new HttpServiceClient(
                receiptsServiceConfig.HostUrl, 
                flurlClientFactory,
                requestRetryPolicyConfig);
        }

        public async Task RecognizeReceipt(Commands.Receipt.V1.RecognizeReceipt command) =>
            await _httpServiceClient.Post(
                "api/v1/receipts/commands/recognize", 
                command.CorrelationId,
                command);

        public async Task<Queries.Receipts.V1.ReadModels.Receipt> GetRecognizedReceipt(Queries.Receipts.V1.GetRecognizedReceipt query) => 
            
            await _httpServiceClient.Get<Queries.Receipts.V1.ReadModels.Receipt>(
                "api/v1/receipts/queries/get-recognized-receipt",
                query.CorrelationId,
                ("ReceiptId", query.ReceiptId));

        public async Task<Queries.Receipts.V1.ReadModels.ReceiptRecognitionProcessState> GetReceiptRecognitionProcessStatus(
                Queries.Receipts.V1.GetReceiptRecognitionProcessStatus query) =>

            await _httpServiceClient.Get<Queries.Receipts.V1.ReadModels.ReceiptRecognitionProcessState>(
                "api/v1/receipts/queries/get-receipt-recognition-process-status",
                query.CorrelationId,
                ("ReceiptId", query.ReceiptId));
    }
}
