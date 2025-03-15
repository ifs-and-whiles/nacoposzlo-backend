using System;
using System.Threading.Tasks;
using System.Transactions;
using Billy.CQRS;
using Billy.Metrics;
using Billy.Receipts.API.Queries;
using Billy.Receipts.Contracts;
using Billy.Receipts.Domain;
using Marten;
using Marten.Services;

namespace Billy.Receipts.ReceiptRecognition
{
    public class MartenReceiptRecognitionProcessStateStore : IReceiptRecognitionProcessStateStore
    {
        private readonly IDocumentStore _documentStore;
        

        public MartenReceiptRecognitionProcessStateStore(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public async Task MarkRecognitionAsStarted(Guid receiptId, string userId)
        {
            var model = new Queries.Receipts.V1.ReadModels.ReceiptRecognitionProcessState()
            {
                Id = receiptId,
                UserId = userId,
                OperationStartDate = DateTimeOffset.UtcNow,
                OperationEndDate = null,
                Status = Queries.Receipts.V1.ReadModels.ReceiptRecognitionProcessStatus.RecognitionStarted
            };

            using (new DbQueryTimer("mark_recognition_as_started"))
            using (var session = _documentStore.OpenSession())
            {
                session.Store(model);

                await session.SaveChangesAsync();
            }
        }

        public async Task MarkRecognitionAsFailed(Guid receiptId)
        {
            using (new DbQueryTimer("mark_recognition_as_failed")) 
            using (var session = _documentStore.OpenSession())
            {
                var processState = await GetOrThrow(receiptId, session);

                processState.OperationEndDate = DateTimeOffset.UtcNow;
                processState.Status = Queries.Receipts.V1.ReadModels.ReceiptRecognitionProcessStatus.RecognitionFailed;

                session.Update(processState);

                await session.SaveChangesAsync();
            }
        }

        private async Task<Queries.Receipts.V1.ReadModels.ReceiptRecognitionProcessState> GetOrThrow(
            Guid receiptId, 
            IDocumentSession session)
        {
            var processState = await session.LoadAsync<Queries.Receipts.V1.ReadModels.ReceiptRecognitionProcessState>(
                receiptId);

            if (processState == null)
                throw new NotFoundException($"ReceiptRecognitionProcessState for receipt id {receiptId} does not exist");

            return processState;
        }
    }

}
