using System;
using System.Threading.Tasks;
using Billy.CQRS;
using Requests = Billy.Users.Contracts.Queries.Users.V1;
using ReadModels = Billy.Users.Contracts.Queries.Users.V1.ReadModels;

namespace Billy.Receipts.UsersService
{
    // Class should use Users Service via HTTP. To simplify solution, users service is a part of billy service.
    public class UsersServiceClient
    {
        //Interface used to inverse dependencies and don't include Users project. Main IOC controls dependencies
        private readonly IHandler<Requests.GetUserReceiptsRecognitionUsageStatus, ReadModels.ReceiptsRecognitionUsage> 
            _queryHandler;

        public UsersServiceClient(
            IHandler<Requests.GetUserReceiptsRecognitionUsageStatus, ReadModels.ReceiptsRecognitionUsage> queryHandler)
        {
            _queryHandler = queryHandler;
        }

        public async Task<ReadModels.ReceiptsRecognitionUsage> GetReceiptsRecognitionUsageStatus(
            string userId,
            Guid correlationId)
        {
            return await _queryHandler.Handle(new MessageContext<Requests.GetUserReceiptsRecognitionUsageStatus>
            {
                Message = new Requests.GetUserReceiptsRecognitionUsageStatus()
                {
                    GlobalUserIdentifier = userId
                },
                CorrelationId = correlationId
            });
        }
    }
}