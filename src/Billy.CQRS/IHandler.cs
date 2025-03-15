using System.Threading.Tasks;

namespace Billy.CQRS
{
    public interface IHandler<TCommand, TResult>
    {
        Task<TResult> Handle(MessageContext<TCommand> context);
    }

    public interface IHandler<TCommand>
    {
        Task Handle(MessageContext<TCommand> context);
    }
}