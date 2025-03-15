using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using MobileApiContract = Billy.MobileApi.Contracts.Logs.Contract.Mobile.V1.Logs;

namespace Billy.MobileApi.API
{
    [AllowAnonymous]
    [ApiController, Route(MobileApiRouteConsts.Prefix + "/v1/logs")]
    public class LogsMobileApiController : MobileApiControllerBase
    {
        private readonly ILogger _logger = Log.ForContext<LogsMobileApiController>();
        
        [HttpPost]
        [Route("add-error-log")]
        public async Task AddErrorLog(
            [FromBody]MobileApiContract.AddErrorLogRequest request)
        {
            _logger.Error("Mobile app error. " +
                          "GlobalUserIdentifier: {UserIdentifier}." +
                          "MobileDetails: {@MobileDetails}" +
                          "LogBody: {@LogBody}", 
                request.UserIdentifier, 
                request.MobileDetails,
                request.LogBody);
        }
    }
}