using System.Threading.Tasks;
using Billy.CQRS;
using Billy.EventSourcing;
using Serilog;

namespace Billy.Users.API.Commands
{
    public class ResetReceiptsRecognitionCurrentPackageCounterCommandHandler : IHandler<
        Contracts.Commands.Users.V1.ResetReceiptsRecognitionCurrentPackageCounter>
    {
        private readonly ApplicationService<Domain.User,Domain.UserStreamId> _applicationService;
        private ILogger _logger = Log.ForContext<IncreaseReceiptsRecognitionCurrentPackageCounterCommandHandler>();

        public ResetReceiptsRecognitionCurrentPackageCounterCommandHandler(
            ApplicationService<Domain.User,Domain.UserStreamId> applicationService)
        {
            _applicationService = applicationService;
        }
        
        public async Task Handle(
            MessageContext<Contracts.Commands.Users.V1.ResetReceiptsRecognitionCurrentPackageCounter> context)
        {
            await _applicationService.Update(
                cmd => Domain.UserStreamId.From(Domain.GlobalUserIdentifier.From(context.Message.GlobalUserIdentifier)),
                (expense, cmd) => expense.ResetReceiptsRecognitionCurrentPackageCounter(),
                context.Message);

            _logger
                .ForContext("GlobalUserIdentifier", context.Message.GlobalUserIdentifier)
                .ForContext("CorrelationId", context.CorrelationId)
                .Information("Receipts recognition current package counter has been reset.");
        }
    }
}