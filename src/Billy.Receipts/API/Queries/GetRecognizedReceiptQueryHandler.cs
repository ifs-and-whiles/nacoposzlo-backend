using System.Linq;
using System.Threading.Tasks;
using Billy.CQRS;
using Billy.Metrics;
using Billy.Receipts.Domain;
using Marten;
using Requests = Billy.Receipts.Contracts.Queries.Receipts.V1;
using ReadModel = Billy.Receipts.Contracts.Queries.Receipts.V1.ReadModels;

namespace Billy.Receipts.API.Queries
{
    public class GetRecognizedReceiptQueryHandler : IHandler<
        Requests.GetRecognizedReceipt, 
        ReadModel.Receipt>
    {
        private readonly IDocumentStore _documentStore;
        public GetRecognizedReceiptQueryHandler(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }
        
        public async Task<ReadModel.Receipt> Handle(
            MessageContext<Requests.GetRecognizedReceipt> context)
        {
            using (new DbQueryTimer("get_recognized_receipt"))
            using (var session = _documentStore.LightweightSession())
            {
                var receipt = await session.Query<ReadModel.Receipt>()
                    .Where(x => x.Id == context.Message.ReceiptId)
                    .FirstOrDefaultAsync();

                if (receipt == null)
                    throw new NotFoundException($"Recognized receipt ReceiptId: {context.Message.ReceiptId} does not exist");

                return receipt;
            }
        }
    }
}