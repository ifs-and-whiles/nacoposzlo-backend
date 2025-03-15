using System;
using System.Threading.Tasks;

namespace Billy.EventSourcing
{
    public interface ISubscription
    {
        Task Project(object @event);
    }
}