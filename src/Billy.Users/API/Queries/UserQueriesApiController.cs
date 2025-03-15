using System.Threading.Tasks;
using Billy.CQRS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Requests = Billy.Users.Contracts.Queries.Users.V1;
using ReadModels = Billy.Users.Contracts.Queries.Users.V1.ReadModels;

namespace Billy.Users.API.Queries
{
    [Authorize(Policy = "ServiceUsers")]
    [ApiController, Route("users-api/v1/users/queries")]
    public class UserQueriesApiController: UserApiControllerBase
    {
        private readonly IHandler<Requests.GetUser, ReadModels.User> 
            _getUserQueryHandler;

        private readonly IHandler<Requests.GetUserReceiptsRecognitionUsageStatus, ReadModels.ReceiptsRecognitionUsage> 
            _getUserReceiptsRecognitionUsageStatusQueryHandler;

        public UserQueriesApiController(
            IHandler<Requests.GetUser, ReadModels.User> 
                getUserQueryHandler,
            IHandler<Requests.GetUserReceiptsRecognitionUsageStatus, ReadModels.ReceiptsRecognitionUsage> 
                getUserReceiptsRecognitionUsageStatusQueryHandler)
        {
            _getUserQueryHandler = getUserQueryHandler;
            _getUserReceiptsRecognitionUsageStatusQueryHandler = getUserReceiptsRecognitionUsageStatusQueryHandler;
        }
        
        [HttpGet]
        [Route("get-single")]
        public async Task<ReadModels.User> GetUser(
            [FromQuery]Requests.GetUser query)
        {
            return await _getUserQueryHandler
                .Handle(new MessageContext<Requests.GetUser>
                {
                    Message = query,
                    CorrelationId = GetRequestCorrelationId()
                });
        }

        [HttpGet]
        [Route("get-receipts-recognition-usage-status")]
        public async Task<ReadModels.ReceiptsRecognitionUsage> GetUserReceiptsRecognitionUsageStatus(
            [FromQuery]Requests.GetUserReceiptsRecognitionUsageStatus query)
        {
            return await _getUserReceiptsRecognitionUsageStatusQueryHandler
                .Handle(new MessageContext<Requests.GetUserReceiptsRecognitionUsageStatus>
                {
                    Message = query,
                    CorrelationId = GetRequestCorrelationId()
                });
        }
    }
}
