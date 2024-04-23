using PollyDistributedCaching.IntegrationTests;
using Xunit;

namespace JourneyCombiner.IntegrationTests;

[CollectionDefinition("WebApplicationFactory Collection")]
public class WebApplicationFactoryCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
