using System.Threading.Tasks;
using Billy.CQRS;
using Billy.Receipts.Domain;
using Requests = Billy.Receipts.Contracts.Queries.Receipts.V1;
using ReadModel = Billy.Receipts.Contracts.Queries.Receipts.V1.ReadModels;

namespace Billy.Receipts.API.Queries
{
    public class GetReceiptImageQueryHandler : IHandler<
        Requests.GetReceiptImage,
        ReadModel.ReceiptImage>
    {
        private readonly IReceiptStore _receiptStore;

        public GetReceiptImageQueryHandler(IReceiptStore receiptStore)
        {
            _receiptStore = receiptStore;
        }
        
        public async Task<ReadModel.ReceiptImage> Handle(MessageContext<Requests.GetReceiptImage> context)
        {
            var imageStream = await _receiptStore.GetReceiptImageStream(context.Message.ReceiptId);
            return new ReadModel.ReceiptImage()
            {
                ContentType = "image/png",
                Image = imageStream
            };
        }
    }
}