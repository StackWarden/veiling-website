namespace backend.tests;

using System.Net;
using FluentAssertions;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

public class AuctionItemTests : IClassFixture<TestFactory>
{
    private readonly HttpClient _client;

    public AuctionItemTests(TestFactory factory)
    {
        _client = factory.CreateClient();
    }

    // --------------------------------------------------GET REQUESTS--------------------------------------------------
    // GET /AuctionItem test
    [Fact(DisplayName = "[GET /AuctionItem returns 200 OK]")]
    public async Task GetAuctionItem()
    {
        var response = await _client.GetAsync("/AuctionItem");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
