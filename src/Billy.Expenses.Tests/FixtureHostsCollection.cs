using Billy.IntegrationTests.Infrastructure;
using Xunit;

namespace Billy.Expenses.Tests
{
    [CollectionDefinition(Name)]
    public class FixtureHostsCollection : ICollectionFixture<HostFixture>
    {
        public const string Name = "Hosts collection";
    }
}
