using System.Threading.Tasks;
using Billy.CQRS;
using Billy.MobileApi.Contracts.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsersMobileApiContract = Billy.MobileApi.Contracts.Users.Contract.Mobile.V1.Users;
using Commands = Billy.Users.Contracts.Commands.Users.V1;
using Queries = Billy.Users.Contracts.Queries.Users.V1;
using ReadModel = Billy.Users.Contracts.Queries.Users.V1.ReadModels;

namespace Billy.MobileApi.API
{
    [Authorize(Policy = "ExternalApiUsers")]
    [ApiController, Route(MobileApiRouteConsts.Prefix + "/v1/users")]
    public class UsersMobileApiController : MobileApiControllerBase
    {
        private readonly IHandler<Commands.Create> _createUserCommandHandler;
        private readonly IHandler<Queries.GetUser, ReadModel.User> _getUserQueryHandler;

        public UsersMobileApiController(
            IHandler<Commands.Create> createUserCommandHandler,
            IHandler<Queries.GetUser, ReadModel.User> getUserQueryHandler)
        {
            _createUserCommandHandler = createUserCommandHandler;
            _getUserQueryHandler = getUserQueryHandler;
        }

        [HttpPost, Route("register-me")]
        public async Task RegisterMe(UsersMobileApiContract.RegisterMeRequest registerMeRequest)
        {
            await _createUserCommandHandler
                .Handle(new MessageContext<Commands.Create>
                {
                    Message = new Commands.Create
                    {
                        GlobalUserIdentifier = GetUserId(),
                        WasTermsAndPrivacyPolicyAccepted = registerMeRequest.WasTermsAndPrivacyPolicyAccepted,
                        DateOfConsents = registerMeRequest.DateOfConsents
                    },
                    CorrelationId = GetRequestCorrelationId()
                });
        }


        [HttpGet, Route("get-my-details")]
        public async Task<UsersMobileApiContract.User> GetMyDetails()
        {
            var userDetails = await _getUserQueryHandler
                .Handle(new MessageContext<Queries.GetUser>
                {
                    Message = new Queries.GetUser
                    {
                        GlobalUserIdentifier = GetUserId()
                    },
                    CorrelationId = GetRequestCorrelationId(),
                });
            
            return new UsersMobileApiContract.User()
            {
                DisplayAds = userDetails.DisplayAds,
                ReceiptsRecognitionUsage = new UsersMobileApiContract.ReceiptsRecognitionUsage()
                {
                    CurrentPackageCounter = userDetails.ReceiptsRecognitionUsage.CurrentPackageCounter,
                    Limit = userDetails.ReceiptsRecognitionUsage.Limit,
                    LimitExceeded = userDetails.ReceiptsRecognitionUsage.LimitExceeded,
                    TotalUtilizationCounter = userDetails.ReceiptsRecognitionUsage.TotalUtilizationCounter
                }
            };
        }
    }
}