namespace PostTrade.IntegrationTests.Tests;

[Collection("Integration")]
public class HealthApiTests : BaseIntegrationTest
{
    public HealthApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    [SkippableFact]
    public async Task GetHealth_Returns200Ok()
    {
        var response = await Client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
