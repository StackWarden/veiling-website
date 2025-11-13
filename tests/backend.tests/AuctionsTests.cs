namespace backend.tests;

using System.Net;
using FluentAssertions;
using Xunit;

public class AuctionTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public AuctionTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "[GET /auctions returns 200 OK]")]
    public async Task GetAuctions()
    {
        var response = await _client.GetAsync("/auctions");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
