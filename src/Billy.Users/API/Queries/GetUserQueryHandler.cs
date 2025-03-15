using System.Linq;
using System.Threading.Tasks;
using Billy.CQRS;
using Billy.Metrics;
using Marten;

namespace Billy.Users.API.Queries
{
    public class GetUserQueryHandler : IHandler<
        Contracts.Queries.Users.V1.GetUser, 
        Contracts.Queries.Users.V1.ReadModels.User>
    {
        private readonly IDocumentStore _documentStore;

        public GetUserQueryHandler(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }
        
        public async Task<Contracts.Queries.Users.V1.ReadModels.User> Handle(
            MessageContext<Contracts.Queries.Users.V1.GetUser> context)
        {
            using (new DbQueryTimer("get_single_user"))
            using (var session = _documentStore.OpenSession())
            {
                var user = await session.Query<Contracts.Queries.Users.V1.ReadModels.User>()
                    .Where(x => x.GlobalUserIdentifier == context.Message.GlobalUserIdentifier)
                    .FirstOrDefaultAsync();

                if (user == null)
                    throw new NotFoundException(
                        $"User with Id {context.Message.GlobalUserIdentifier} not found");

                return user;
            }
        }
    }
}