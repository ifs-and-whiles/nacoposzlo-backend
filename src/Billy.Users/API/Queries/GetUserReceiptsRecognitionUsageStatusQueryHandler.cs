using System;
using System.Linq;
using System.Threading.Tasks;
using Billy.CQRS;
using Billy.Metrics;
using Marten;
using Marten.Linq.QueryHandlers;
using Requests = Billy.Users.Contracts.Queries.Users.V1;
using ReadModels = Billy.Users.Contracts.Queries.Users.V1.ReadModels;
namespace Billy.Users.API.Queries
{
    public class GetUserReceiptsRecognitionUsageStatusQueryHandler : IHandler<
        Requests.GetUserReceiptsRecognitionUsageStatus,
        ReadModels.ReceiptsRecognitionUsage>
    {
        private readonly IDocumentStore _documentStore;

        public GetUserReceiptsRecognitionUsageStatusQueryHandler(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }
        
        public async Task<ReadModels.ReceiptsRecognitionUsage> Handle(
            MessageContext<Requests.GetUserReceiptsRecognitionUsageStatus> context)
        {
            using (new DbQueryTimer("get_receipts_recognition_usage_status"))
            using (var session = _documentStore.OpenSession())
            {
                var user = await session
                    .Query<ReadModels.User>()
                    .Where(x => x.GlobalUserIdentifier == context.Message.GlobalUserIdentifier)
                    .Select(u => new ReadModels.ReceiptsRecognitionUsage()
                    {
                        LimitExceeded = u.ReceiptsRecognitionUsage.LimitExceeded,
                        CurrentPackageCounter = u.ReceiptsRecognitionUsage.CurrentPackageCounter,
                        Limit = u.ReceiptsRecognitionUsage.Limit,
                        TotalUtilizationCounter = u.ReceiptsRecognitionUsage.TotalUtilizationCounter
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                    throw new NotFoundException(
                        $"User with Id {context.Message.GlobalUserIdentifier} not found");

                return user;
            }
        }
    }
}