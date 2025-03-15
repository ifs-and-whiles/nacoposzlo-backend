using System.Threading.Tasks;
using Billy.CQRS;
using Billy.EventSourcing;
using Billy.IdGeneration;
using Serilog;

namespace Billy.Users.API.Commands
{
    public class CreateUserCommandHandler : IHandler<
        Contracts.Commands.Users.V1.Create>
    {
        private readonly ApplicationService<Domain.User, Domain.UserStreamId> _applicationService;
        private readonly IIdGenerator _idGenerator;
        private ILogger _logger = Log.ForContext<CreateUserCommandHandler>();
        
        public CreateUserCommandHandler(
            ApplicationService<Domain.User,Domain.UserStreamId> applicationService, 
            IIdGenerator idGenerator)
        {
            _applicationService = applicationService;
            _idGenerator = idGenerator;
        }

        public async Task Handle(
            MessageContext<Contracts.Commands.Users.V1.Create> context)
        {
            var userStreamId = Domain.UserStreamId.From(
                Domain.GlobalUserIdentifier.From(
                    context.Message.GlobalUserIdentifier));

            if (await _applicationService.Exists(userStreamId))
            {
                _logger.Information(
                    "Registration has been skipped. User with {GlobalUserIdentifier} already exists.",
                    context.Message.GlobalUserIdentifier);
            }
            else
            {
                var userId = _idGenerator.NewId();

                await _applicationService.Create(
                    cmd => userStreamId,
                    (cmd) => Domain.User.Create(
                        Domain.UserId.From(userId),
                        Domain.GlobalUserIdentifier.From(
                            context.Message.GlobalUserIdentifier),
                        Domain.ReceiptsRecognitionUsage.From(
                            Domain.ReceiptsRecognitionLimit.From(
                                context.Message.ReceiptsRecognitionLimit),
                            Domain.ReceiptsRecognitionCurrentPackageCounter.From(
                                context.Message.ReceiptsRecognitionCurrentPackageCounter)),
                        Domain.TermsAndPrivacyPolicy.From(
                            context.Message.WasTermsAndPrivacyPolicyAccepted,
                            context.Message.DateOfConsents)),
                    context.Message);

                _logger
                    .ForContext("GlobalUserIdentifier", context.Message.GlobalUserIdentifier)
                    .ForContext("CorrelationId", context.CorrelationId)
                    .Information("User created");
            }
        }
    }
}