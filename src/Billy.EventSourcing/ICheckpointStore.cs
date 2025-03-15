using System.Threading.Tasks;

namespace Billy.EventSourcing
{
    public interface ICheckpointStore
    {
        Task<long?> GetCheckpoint();
        Task StoreCheckpoint(long? checkpoint);
    }
}