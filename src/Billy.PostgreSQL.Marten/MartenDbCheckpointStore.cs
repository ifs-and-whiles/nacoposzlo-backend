using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Billy.EventSourcing;
using EventStore.ClientAPI;
using Marten;

namespace Billy.PostgreSQL.Marten
{
    public class MartenDbCheckpointStore : ICheckpointStore
    {
        private readonly IDocumentStore _documentStore;
        private readonly string _checkpointName;

        public MartenDbCheckpointStore(IDocumentStore documentStore,
            string checkpointName)
        {
            _documentStore = documentStore;
            _checkpointName = checkpointName;
        }

        public async Task<long?> GetCheckpoint()
        {
            using var session = _documentStore.OpenSession();

            var checkpoint = await session.LoadAsync<Checkpoint>(_checkpointName);
            return checkpoint?.Position ?? AllCheckpoint.AllStart?.CommitPosition;
        }

        public async Task StoreCheckpoint(long? position)
        {
            using var session = _documentStore.OpenSession();

            var checkpoint = await session.LoadAsync<Checkpoint>(_checkpointName);

            if (checkpoint == null)
            {
                checkpoint = new Checkpoint
                {
                    Id = _checkpointName
                };
            }

            checkpoint.Position = position;

            session.Store(checkpoint);
            session.SaveChanges();
        }

        public class Checkpoint
        {
            public string Id { get; set; }
            public long? Position { get; set; }
        }
    }
}
