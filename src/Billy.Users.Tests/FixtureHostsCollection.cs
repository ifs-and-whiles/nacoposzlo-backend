using Billy.IntegrationTests.Infrastructure;
using Xunit;

namespace Billy.Users.Tests
{
    [CollectionDefinition(Name)]
    public class FixtureHostsCollection : ICollectionFixture<HostFixture>
    {
        public const string Name = "Hosts collection";
    }
}
