using System.Threading.Tasks;
using Billy.CQRS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billy.Users.API.Commands
{
    [Authorize(Policy = "ServiceUsers")]
    [ApiController, Route("users-api/v1/users/commands")]
    public class UserCommandApiController: UserApiControllerBase
    {
        private readonly IHandler<Contracts.Commands.Users.V1.AssignReceiptsRecognitionLimit> 
            _assignReceiptsRecognitionLimitCommandHandler;

        private readonly IHandler<Contracts.Commands.Users.V1.Create> 
            _createUserCommandHandler;

        private readonly IHandler<Contracts.Commands.Users.V1.IncreaseReceiptsRecognitionCurrentPackageCounter> 
            _increaseReceiptsRecognitionCurrentPackageCounterCommandHandler;

        private readonly IHandler<Contracts.Commands.Users.V1.ResetReceiptsRecognitionCurrentPackageCounter>
            _recognitionCurrentPackageCounterCommandHandler;

        private readonly IHandler<Contracts.Commands.Users.V1.ResetReceiptsRecognitionCurrentPackageCounterForAllUsers> 
            _resetReceiptsRecognitionCurrentPackageCounterForAllUsersCommandHandler;

        public UserCommandApiController(
            
            IHandler<Contracts.Commands.Users.V1.AssignReceiptsRecognitionLimit> 
                assignReceiptsRecognitionLimitCommandHandler,
            
            IHandler<Contracts.Commands.Users.V1.Create> 
                createUserCommandHandler,
            
            IHandler<Contracts.Commands.Users.V1.IncreaseReceiptsRecognitionCurrentPackageCounter> 
                increaseReceiptsRecognitionCurrentPackageCounterCommandHandler,
            
            IHandler<Contracts.Commands.Users.V1.ResetReceiptsRecognitionCurrentPackageCounter> 
                recognitionCurrentPackageCounterCommandHandler,
            
            IHandler<Contracts.Commands.Users.V1.ResetReceiptsRecognitionCurrentPackageCounterForAllUsers>
                resetReceiptsRecognitionCurrentPackageCounterForAllUsersCommandHandler)
        {
            _assignReceiptsRecognitionLimitCommandHandler = assignReceiptsRecognitionLimitCommandHandler;
            _createUserCommandHandler = createUserCommandHandler;
            _increaseReceiptsRecognitionCurrentPackageCounterCommandHandler = increaseReceiptsRecognitionCurrentPackageCounterCommandHandler;
            _recognitionCurrentPackageCounterCommandHandler = recognitionCurrentPackageCounterCommandHandler;
            _resetReceiptsRecognitionCurrentPackageCounterForAllUsersCommandHandler = resetReceiptsRecognitionCurrentPackageCounterForAllUsersCommandHandler;
        }
        
        [HttpPost, Route("create")]
        public async Task CreateUser(
            Contracts.Commands.Users.V1.Create command)
        {
            await _createUserCommandHandler
                .Handle(new MessageContext<Contracts.Commands.Users.V1.Create>
                {
                    Message = command,
                    CorrelationId = GetRequestCorrelationId()
                });
        }

        [HttpPost, Route("assign-receipts-recognition-limit")]
        public async Task AssignReceiptsRecognitionLimit(
            Contracts.Commands.Users.V1.AssignReceiptsRecognitionLimit command)
        {
            await _assignReceiptsRecognitionLimitCommandHandler
                .Handle(new MessageContext<Contracts.Commands.Users.V1.AssignReceiptsRecognitionLimit>
                {
                    Message = command,
                    CorrelationId = GetRequestCorrelationId()
                });
        }

        [HttpPost, Route("increase-receipts-recognition-current-package-counter")]
        public async Task IncreaseReceiptsRecognitionCurrentPackageCounter(
            Contracts.Commands.Users.V1.IncreaseReceiptsRecognitionCurrentPackageCounter command)
        {
            await _increaseReceiptsRecognitionCurrentPackageCounterCommandHandler
                .Handle(new MessageContext<Contracts.Commands.Users.V1.IncreaseReceiptsRecognitionCurrentPackageCounter>
                {
                    Message = command,
                    CorrelationId = GetRequestCorrelationId()
                });
        }

        [HttpPost, Route("reset-receipts-recognition-current-package-counter")]
        public async Task ResetReceiptsRecognitionCurrentPackageCounter(
            Contracts.Commands.Users.V1.ResetReceiptsRecognitionCurrentPackageCounter command)
        {
            await _recognitionCurrentPackageCounterCommandHandler
                .Handle(new MessageContext<Contracts.Commands.Users.V1.ResetReceiptsRecognitionCurrentPackageCounter>
                {
                    Message = command,
                    CorrelationId = GetRequestCorrelationId()
                });
        }

        [HttpPost, Route("reset-receipts-recognition-current-package-counter-for-all-users")]
        public async Task ResetReceiptsRecognitionCurrentPackageCounterForAllUsers(
            Contracts.Commands.Users.V1.ResetReceiptsRecognitionCurrentPackageCounterForAllUsers command)
        {
            await _resetReceiptsRecognitionCurrentPackageCounterForAllUsersCommandHandler
                .Handle(new MessageContext<Contracts.Commands.Users.V1.ResetReceiptsRecognitionCurrentPackageCounterForAllUsers>
                {
                    Message = command,
                    CorrelationId = GetRequestCorrelationId()
                });
        }
    }
}