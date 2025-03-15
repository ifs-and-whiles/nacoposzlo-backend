using System;
using System.Threading;
using System.Threading.Tasks;
using Billy.CQRS;
using Billy.Metrics;
using Billy.Users.Contracts;
using Billy.Users.Domain;
using Marten;
using Marten.Events.Projections;
using Marten.Events.Projections.Async;
using Marten.Storage;

namespace Billy.Users.Projections
{
    public class UserProjection : IProjection
    {
        
        public Type[] Consumes { get; } =
        {
            typeof(Events.Users.V1.UserAdded),
            typeof(Events.Users.V1.ReceiptsRecognitionCounterZeroed),
            typeof(Events.Users.V1.ReceiptsRecognitionLimitAssigned),
            typeof(Events.Users.V1.ReceiptsRecognitionCurrentPackageCounterIncreased),
            typeof(Events.Users.V1.ReceiptsRecognitionLimitReached)
        };

        public void Apply(IDocumentSession session, EventPage page) =>
            throw new NotImplementedException("Synchronous version is not used");

        public async Task ApplyAsync(IDocumentSession session, EventPage page, CancellationToken token)
        {
            foreach (var @event in page.Events)
                await Apply(@event.Data, session);
        }

        private async Task Apply(object @event, IDocumentSession session)
        {
            using (new ProjectionEventTimer(nameof(UserProjection), @event))
            {
                switch (@event)
                {
                    case Events.Users.V1.UserAdded e:
                        await Create(e);
                        break;
                    case Events.Users.V1.ReceiptsRecognitionCounterZeroed e:
                        await Update(e.Id, user =>
                        {
                            user.ReceiptsRecognitionUsage.CurrentPackageCounter = 0;
                            user.DisplayAds = false;
                            user.ReceiptsRecognitionUsage.LimitExceeded = false;
                        });
                        break;
                    case Events.Users.V1.ReceiptsRecognitionLimitAssigned e:
                        await Update(e.Id, user =>
                        {
                            user.ReceiptsRecognitionUsage.Limit = e.Limit;
                            user.DisplayAds = e.Limit == user.ReceiptsRecognitionUsage.CurrentPackageCounter;
                            user.ReceiptsRecognitionUsage.LimitExceeded = e.Limit == user.ReceiptsRecognitionUsage.CurrentPackageCounter;
                        });
                        break;
                    case Events.Users.V1.ReceiptsRecognitionCurrentPackageCounterIncreased e:
                        await Update(e.Id, user =>
                        {
                            user.ReceiptsRecognitionUsage.CurrentPackageCounter++;
                            user.ReceiptsRecognitionUsage.TotalUtilizationCounter++;
                        });
                        break;
                    case Events.Users.V1.ReceiptsRecognitionLimitReached e:
                        await Update(e.Id, user =>
                        {
                            user.DisplayAds = true;
                            user.ReceiptsRecognitionUsage.LimitExceeded = true;
                        });
                        break;
                }
            }


            async Task Create(Events.Users.V1.UserAdded e)
            {
                var user = new Contracts.Queries.Users.V1.ReadModels.User()
                {
                    Id = e.Id,
                    GlobalUserIdentifier = e.GlobalUserIdentifier,
                    ReceiptsRecognitionUsage = new Contracts.Queries.Users.V1.ReadModels.ReceiptsRecognitionUsage()
                    {
                        CurrentPackageCounter = e.ReceiptsRecognitionCurrentPackageCounter,
                        Limit = e.ReceiptsRecognitionLimit,
                        TotalUtilizationCounter = e.ReceiptsRecognitionCurrentPackageCounter
                    },
                    TermsAndPrivacyPolicy = new Queries.Users.V1.ReadModels.TermsAndPrivacyPolicy()
                    {
                        DateOfConsents = e.TermsAndPrivacyPolicy.DateOfConsents,
                        WasTermsAndPrivacyPolicyAccepted = e.TermsAndPrivacyPolicy.WasTermsAndPrivacyPolicyAccepted
                    },
                    DisplayAds = false
                };

                session.Store(user);
                await session.SaveChangesAsync();
            }
            
            async Task Update(Guid id, Action<Contracts.Queries.Users.V1.ReadModels.User> updateAction)
            {
                var user = await LoadOrTrow(session, id);

                updateAction.Invoke(user);

                session.Store(user);

                await session.SaveChangesAsync();
            }
        }

        private static async Task<Contracts.Queries.Users.V1.ReadModels.User> LoadOrTrow(
            IDocumentSession session, Guid id)
        {
            var result = await session.LoadAsync<Contracts.Queries.Users.V1.ReadModels.User>(id);
            if(result == null)
                throw new NotFoundException($"User for {id} does not exist");
            return result;
        }
        
        public void EnsureStorageExists(ITenant tenant)
        {
        }

        public AsyncOptions AsyncOptions { get; } = new AsyncOptions();
    }
}