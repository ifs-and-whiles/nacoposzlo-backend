using System.Threading.Tasks;
using Billy.CQRS;
using Billy.Metrics;
using Billy.Receipts.Contracts;
using MassTransit;
using Serilog;
using Serilog.Context;
using Commands = Billy.Users.Contracts.Commands;

namespace Billy.Users.ReceiptsRecognition
{
    public class ReceiptRecognitionCompletedConsumer : IConsumer<Messages.V1.ReceiptRecognitionCompletedEvent>
    {
        private readonly IHandler<Commands.Users.V1.IncreaseReceiptsRecognitionCurrentPackageCounter> _commandHandler;
        private ILogger _logger = Log.ForContext<ReceiptRecognitionCompletedConsumer>();
        
        public ReceiptRecognitionCompletedConsumer(
            IHandler<Commands.Users.V1.IncreaseReceiptsRecognitionCurrentPackageCounter> 
                increaseReceiptsRecognitionCurrentPackageCounterCommandHandler)
        {
            _commandHandler = increaseReceiptsRecognitionCurrentPackageCounterCommandHandler;
        }
        
        public async Task Consume(ConsumeContext<Messages.V1.ReceiptRecognitionCompletedEvent> context)
        {
            //I need to find a way how to create generic decorator for mass transit consumer.
            using (LogContext.PushProperty("CorrelationId", context.CorrelationId))
            using (LogContext.PushProperty("GlobalUserIdentifier", context.Message.GlobalUserIdentifier))
            using (new MessageProcessingTimer(context.Message.GetType().Name))
            {
                _logger.Information(
                    "Receipt {receiptId} has been successfully recognized.",
                    context.Message.ReceiptId);

                await _commandHandler.Handle(new MessageContext<Commands.Users.V1.IncreaseReceiptsRecognitionCurrentPackageCounter>
                {
                    Message = new Commands.Users.V1.IncreaseReceiptsRecognitionCurrentPackageCounter()
                    {
                        GlobalUserIdentifier = context.Message.GlobalUserIdentifier
                    },
                    CorrelationId = context.Message.CorrelationId
                });
            }
        }
    }
}