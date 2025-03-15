namespace Billy.EventSourcing
{
    public interface IInternalEventHandler
    {
        void Handle(object @event);
    }
}