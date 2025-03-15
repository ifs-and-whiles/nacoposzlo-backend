using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Billy.CQRS;
using Billy.Metrics;
using Billy.Receipts.API.Queries;
using Billy.Receipts.Contracts;
using Billy.Receipts.Domain;
using Marten;
using Marten.Events.Projections;
using Marten.Services;
using MassTransit;
using Serilog;
using Serilog.Context;

namespace Billy.Receipts.ReceiptRecognition.Consumers
{
    public class SaveRecognizedReceiptHandler : IConsumer<Messages.V1.ReceiptRecognizedEvent>
    {
        private readonly IDocumentStore _documentStore;
        private ILogger _logger = Log.ForContext<SaveRecognizedReceiptHandler>();

        public SaveRecognizedReceiptHandler(
            IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public async Task Consume(ConsumeContext<Messages.V1.ReceiptRecognizedEvent> context)
        {
            //I need to find a way how to create generic decorator for mass transit consumer.
            using (LogContext.PushProperty("CorrelationId", context.CorrelationId))
            using (LogContext.PushProperty("GlobalUserIdentifier", context.Message.GlobalUserIdentifier))
            using (new MessageProcessingTimer(context.Message.GetType().Name))
            using (new DbQueryTimer("store_recognized_receipt"))
            using (var session = _documentStore.OpenSession())
            {
                if (await DoesReceiptExistInDatabase(session, context))
                {
                    _logger.Warning(
                        "Receipt {receiptId} already exists in DB. Adding model to db has been skipped",
                        context.Message.ReceiptId);
                }
                else
                {
                    var receiptDbModel = new Queries.Receipts.V1.ReadModels.Receipt()
                    {
                        Id = context.Message.ReceiptId,
                        UserId = context.Message.GlobalUserIdentifier,
                        OriginalOrientation = new Queries.Receipts.V1.ReadModels.Orientation
                        {
                            ValueInRadians = context.Message.RecognizedReceipt.OriginalOrientation.ValueInRadians
                        },
                        Amount = context.Message.RecognizedReceipt.Amount,
                        Date = context.Message.RecognizedReceipt.Date,
                        Seller = context.Message.RecognizedReceipt.Seller,
                        TaxNumber = context.Message.RecognizedReceipt.TaxNumber,
                        Products = context.Message.RecognizedReceipt.Products.Select(p => new Queries.Receipts.V1.ReadModels.Product()
                        {
                            IsRecognized = p.IsRecognized,
                            IsCorrupted = p.IsCorrupted,
                            Name = p.Name,
                            Quantity = p.Quantity,
                            UnitPrice = p.UnitPrice,
                            Amount = p.Amount,
                            BoundingBox = MapMessageToReadModelBoundingBox(p.BoundingBox)
                        }).ToList(),
                        Problems = context.Message.RecognizedReceipt.Problems.Select(p => p switch
                        {
                            (Messages.V1.RecognizedReceipt.Problem.AmountDifferentThanSumOfProducts) => 
                            Queries.Receipts.V1.ReadModels.Problem.AmountDifferentThanSumOfProducts,
                            _ => throw new ReceiptMappingException(message: 
                                $"Receipt recognition problem {Enum.GetName(typeof(RecognizedReceipt.Problem), p)} " +
                                $"does not exist in read model"),
                        }).ToList()
                    };

                    session.Store(receiptDbModel);

                    _logger.Information(
                        "Receipt {receiptId} has been successfully added to DB",
                        context.Message.ReceiptId);
                    
                    await MarkRecognitionAsCompleted(session, context);

                    await session.SaveChangesAsync();
                }
            }

            await context.Publish(new Messages.V1.ReceiptRecognitionCompletedEvent()
            {
                ReceiptId = context.Message.ReceiptId,
                GlobalUserIdentifier = context.Message.GlobalUserIdentifier,
                CorrelationId = context.Message.CorrelationId
            });
        }

        private async Task<bool> DoesReceiptExistInDatabase(
            IDocumentSession documentSession,
            ConsumeContext<Messages.V1.ReceiptRecognizedEvent> context)
        {
            return await documentSession.LoadAsync<Queries.Receipts.V1.ReadModels.Receipt>(context.Message.ReceiptId) !=
                   null;
        }

        private async Task MarkRecognitionAsCompleted(IDocumentSession session,
            ConsumeContext<Messages.V1.ReceiptRecognizedEvent> context)
        {
            var processState = await GetOrThrow(context.Message.ReceiptId, session);

            processState.OperationEndDate = DateTimeOffset.UtcNow;
            processState.Status = Queries.Receipts.V1.ReadModels.ReceiptRecognitionProcessStatus.RecognitionCompleted;

            session.Update(processState);
        }

        private async Task<Queries.Receipts.V1.ReadModels.ReceiptRecognitionProcessState> GetOrThrow(
            Guid receiptId, 
            IDocumentSession session)
        {
            var processState = await session.LoadAsync<Queries.Receipts.V1.ReadModels.ReceiptRecognitionProcessState>(receiptId);

            if (processState == null)
                throw new NotFoundException($"ReceiptRecognitionProcessState for receipt id {receiptId} does not exist");

            return processState;
        }

        private static Queries.Receipts.V1.ReadModels.BoundingBox MapMessageToReadModelBoundingBox(
            Messages.V1.BoundingBox boundingBox)
        {
            return new Queries.Receipts.V1.ReadModels.BoundingBox
            {
                RightTop = MapMessageToReadModelPoint(boundingBox.RightTop),
                LeftTop = MapMessageToReadModelPoint(boundingBox.LeftTop),
                LeftBottom = MapMessageToReadModelPoint(boundingBox.LeftBottom),
                RightBottom = MapMessageToReadModelPoint(boundingBox.RightBottom)
            };
        }

        private static Queries.Receipts.V1.ReadModels.Point MapMessageToReadModelPoint(
            Messages.V1.NormalizedPoint normalizedPoint)
        {
            return new Queries.Receipts.V1.ReadModels.Point
            {
                X = normalizedPoint.X,
                Y = normalizedPoint.Y
            };
        }

        public class ReceiptMappingException : Exception
        {
            public ReceiptMappingException(string message) : base(message)
            {
                
            }
        }
    }
}
