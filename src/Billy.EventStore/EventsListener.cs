using System;
using System.Linq;
using System.Threading.Tasks;
using Billy.EventSourcing;
using Billy.EventStore.Logging;
using EventStore.ClientAPI;

namespace Billy.EventStore
{
    public class EventsListener
    {
        static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        readonly ICheckpointStore _checkpointStore;
        readonly string _subscriptionName;
        readonly IEventStoreConnection _connection;
        readonly ISubscription[] _subscriptions;
        EventStoreAllCatchUpSubscription _subscription;

        public EventsListener(
            IEventStoreConnection connection,
            ICheckpointStore checkpointStore,
            string subscriptionName,
            params ISubscription[] subscriptions)
        {
            _connection = connection;
            _checkpointStore = checkpointStore;
            _subscriptions = subscriptions;
            _subscriptionName = subscriptionName;
        }

        public async Task Start()
        {
            var settings = new CatchUpSubscriptionSettings(
                2000, 500,
                Log.IsDebugEnabled(),
                false, _subscriptionName
            );
            
            Log.Debug("Starting the events listener...");

            var checkpointPosition = await _checkpointStore.GetCheckpoint();
            Log.Debug("Retrieved the checkpoint: {checkpoint}", checkpointPosition);
            _subscription = _connection.SubscribeToAllFrom(
                GetPosition(),
                settings, 
                EventAppeared
            );
            Log.Debug("Subscribed to $all stream");

            Position? GetPosition()
                => checkpointPosition.HasValue
                    ? new Position(checkpointPosition.Value, checkpointPosition.Value)
                    : AllCheckpoint.AllStart; // AllStart indicates that a catch-up subscription should receive all events in the database.
        }


        public async Task WaitForNonStaleResults()
        {
            var counter = 0;
            int waitingIntervalInMilliseconds = 200;
            int maximumNumberOfCheckOperations = 20;
            bool areNotProjectedEvents = true;

            while (areNotProjectedEvents && counter <= maximumNumberOfCheckOperations)
            {
                var checkpointPosition = await _checkpointStore.GetCheckpoint();

                var events = await _connection.ReadAllEventsForwardAsync(
                    GetPosition(), 
                    1000,
                    false);

                var lastNotProcessedEvent = events.Events.GetTheNewestCustomEvent();

                if (lastNotProcessedEvent.OriginalPosition != null &&
                    lastNotProcessedEvent.OriginalPosition.Value.CommitPosition > GetPosition().CommitPosition)
                {
                    counter++;
                    await Task.Delay(waitingIntervalInMilliseconds);
                }
                else
                    areNotProjectedEvents = false;

                Position GetPosition()
                    => checkpointPosition.HasValue
                        ? new Position(checkpointPosition.Value, checkpointPosition.Value)
                        : Position.Start;
            }
        }

        async Task EventAppeared(
            EventStoreCatchUpSubscription _,
            ResolvedEvent resolvedEvent)
        {
            if (resolvedEvent.Event.EventType.StartsWith("$")) return;

            var @event = resolvedEvent.Deserialze();

            Log.Debug("Projecting event {event}", @event.ToString());

            try
            {
                await Task.WhenAll(_subscriptions.Select(x => x.Project(@event)));

                await _checkpointStore.StoreCheckpoint(
                    // ReSharper disable once PossibleInvalidOperationException
                    resolvedEvent.OriginalPosition.Value.CommitPosition
                );
            }
            catch (Exception e)
            {
                Log.Error(e, "Error occured when projecting the event {event}", @event );
                throw;
            }
        }

        public void Stop() => _subscription.Stop();
    }
}