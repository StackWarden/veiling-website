namespace backend.tests;

using System.Net;
using FluentAssertions;
using Xunit;
using System.Net.Http.Json;

public class AuctionTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public AuctionTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    // GET /auctions test
    [Fact(DisplayName = "[GET /auctions returns 200 OK]")]
    public async Task GetAuctions()
    {
        var response = await _client.GetAsync("/auctions");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // POST /auctions test
    [Fact(DisplayName = "[POST /auctions creates an auction]")]
    public async Task CreateAuction()
    {
        var auctioneerId = Guid.NewGuid();
        var startTime = DateTime.Parse("2025-12-01T09:00:00Z");
        var endTime = DateTime.Parse("2025-12-01T10:00:00Z");

        // JSON body as C# object
        var body = new
        {
            auctionneerId = auctioneerId,
            startTime,
            endTime,
            status = "Planned"
        };

        // Act — send POST with JSON
        var response = await _client.PostAsJsonAsync("/auctions", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
