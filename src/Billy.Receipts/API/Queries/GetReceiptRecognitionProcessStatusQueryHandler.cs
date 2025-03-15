using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Marten;
using System.Linq;
using Billy.CQRS;
using Billy.Metrics;
using Billy.Receipts.Domain;
using Requests = Billy.Receipts.Contracts.Queries.Receipts.V1;
using ReadModel = Billy.Receipts.Contracts.Queries.Receipts.V1.ReadModels;

namespace Billy.Receipts.API.Queries
{
    public class GetReceiptRecognitionProcessStatusQueryHandler : IHandler<
        Requests.GetReceiptRecognitionProcessStatus,
        ReadModel.ReceiptRecognitionProcessState>
    {
        private readonly IDocumentStore _documentStore;
        public GetReceiptRecognitionProcessStatusQueryHandler(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }
        
        public async Task<ReadModel.ReceiptRecognitionProcessState> Handle(
            MessageContext<Requests.GetReceiptRecognitionProcessStatus> context)
        {
            using (new DbQueryTimer("get_recognition_process_state"))
            using (var session = _documentStore.LightweightSession())
            {
                var state = await session.Query<ReadModel.ReceiptRecognitionProcessState>()
                    .Where(x => x.Id == context.Message.ReceiptId)
                    .FirstOrDefaultAsync();

                if (state == null)
                    throw new NotFoundException($"Receipt recognition state for {context.Message.ReceiptId} does not exist");

                return state;
            }
        }
    }
}
