namespace PostTrade.IntegrationTests.Infrastructure;

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
    // This class has no code. Its sole purpose is to wire up the
    // [CollectionDefinition] and ICollectionFixture<> so that all
    // [Collection("Integration")] test classes share one factory instance.
}
