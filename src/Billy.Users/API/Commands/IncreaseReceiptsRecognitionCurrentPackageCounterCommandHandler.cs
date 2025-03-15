using System.Threading.Tasks;
using Billy.CQRS;
using Billy.EventSourcing;
using Billy.Users.ReceiptsRecognition;
using Serilog;

namespace Billy.Users.API.Commands
{
    public class IncreaseReceiptsRecognitionCurrentPackageCounterCommandHandler : IHandler<
        Contracts.Commands.Users.V1.IncreaseReceiptsRecognitionCurrentPackageCounter>
    {
        private readonly ApplicationService<Domain.User,Domain.UserStreamId> _applicationService;
        private ILogger _logger = Log.ForContext<IncreaseReceiptsRecognitionCurrentPackageCounterCommandHandler>();

        public IncreaseReceiptsRecognitionCurrentPackageCounterCommandHandler(
            ApplicationService<Domain.User,Domain.UserStreamId> applicationService)
        {
            _applicationService = applicationService;
        }

        public async Task Handle(
            MessageContext<Contracts.Commands.Users.V1.IncreaseReceiptsRecognitionCurrentPackageCounter> context)
        {
            await _applicationService.Update(
                cmd => Domain.UserStreamId.From(
                    Domain.GlobalUserIdentifier.From(
                        context.Message.GlobalUserIdentifier)),
                (user, cmd) => user.IncreaseReceiptsRecognitionCurrentPackageCounter(),
                context.Message);

            _logger
                .ForContext("GlobalUserIdentifier", context.Message.GlobalUserIdentifier)
                .ForContext("CorrelationId", context.CorrelationId)
                .Information("Receipts recognition current package counter has been increased");
        }
    }
}