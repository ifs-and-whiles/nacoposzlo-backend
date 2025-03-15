using System.Threading.Tasks;
using Billy.CQRS;
using Billy.EventSourcing;
using Billy.Users.Domain;
using Serilog;

namespace Billy.Users.API.Commands
{
    public class ResetReceiptsRecognitionCurrentPackageCounterForAllUsersCommandHandler : IHandler<
        Contracts.Commands.Users.V1.ResetReceiptsRecognitionCurrentPackageCounterForAllUsers>
    {
        private readonly ApplicationService<User, UserStreamId> _applicationService;
        private ILogger _logger = Log.ForContext<ResetReceiptsRecognitionCurrentPackageCounterForAllUsersCommandHandler>();


        public ResetReceiptsRecognitionCurrentPackageCounterForAllUsersCommandHandler(
            ApplicationService<User, UserStreamId> applicationService)
        {
            _applicationService = applicationService;
        }
        
        public async Task Handle(
            MessageContext<Contracts.Commands.Users.V1.ResetReceiptsRecognitionCurrentPackageCounterForAllUsers> context)
        {
            await _applicationService.UpdateAll(
                UserStreamId.StreamPrefix,
                (user, cmd) => user.ResetReceiptsRecognitionCurrentPackageCounter(),
                context.Message);

            _logger
                .ForContext("CorrelationId", context.CorrelationId)
                .Information("Receipts recognition current package counter has been reset for all users.");
        }
    }
}