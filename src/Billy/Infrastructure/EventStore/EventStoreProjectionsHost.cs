using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Billy.EventStore;
using EventStore.ClientAPI;
using Microsoft.Extensions.Hosting;

namespace Billy.Infrastructure.EventStore
{
    public class EventStoreProjectionsHost : IHostedService
    {
        readonly IEventStoreConnection _esConnection;
        readonly IEnumerable<EventsListener> _eventsListeners;

        public EventStoreProjectionsHost(
            IEventStoreConnection esConnection,
            IEnumerable<EventsListener> eventsListeners)
        {
            _esConnection = esConnection;
            _eventsListeners = eventsListeners;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _esConnection.ConnectAsync();

            await Task.WhenAll(
                _eventsListeners
                    .Select(projection => projection.Start())
            );
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach(var eventsListener in _eventsListeners)
            {
                eventsListener.Stop();
            }
            _esConnection.Close();
            return Task.CompletedTask;
        }
    }
}
