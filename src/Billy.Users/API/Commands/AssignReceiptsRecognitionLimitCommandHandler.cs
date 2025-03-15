using System.Threading.Tasks;
using Billy.CQRS;
using Billy.EventSourcing;
using Serilog;

namespace Billy.Users.API.Commands
{
    public class AssignReceiptsRecognitionLimitCommandHandler : IHandler<
        Contracts.Commands.Users.V1.AssignReceiptsRecognitionLimit>
    {
        private readonly ApplicationService<Domain.User,Domain.UserStreamId> _applicationService;

        private readonly ILogger _logger = Log.ForContext<AssignReceiptsRecognitionLimitCommandHandler>();

        public AssignReceiptsRecognitionLimitCommandHandler(
            ApplicationService<Domain.User,Domain.UserStreamId> applicationService)
        {
            _applicationService = applicationService;
        }
        
        public async Task Handle(Contracts.Commands.Users.V1.AssignReceiptsRecognitionLimit command)
        {
            await _applicationService.Update(
                cmd => Domain.UserStreamId.From(Domain.GlobalUserIdentifier.From(command.GlobalUserIdentifier)),
                (expense, cmd) => expense.AssignReceiptsRecognitionLimit(
                    Domain.ReceiptsRecognitionLimit.From(command.Limit)),
                command);
        }

        public async Task Handle(MessageContext<Contracts.Commands.Users.V1.AssignReceiptsRecognitionLimit> context)
        {
            await _applicationService.Update(
                cmd => Domain.UserStreamId.From(Domain.GlobalUserIdentifier.From(context.Message.GlobalUserIdentifier)),
                (expense, cmd) => expense.AssignReceiptsRecognitionLimit(
                    Domain.ReceiptsRecognitionLimit.From(context.Message.Limit)),
                context.Message);

            _logger
                .ForContext("GlobalUserIdentifier", context.Message.GlobalUserIdentifier)
                .ForContext("CorrelationId", context.CorrelationId)
                .Information("Receipt recognition limit {limit} assigned", context.Message.Limit);
        }
    }
}