namespace backend.tests;

using System.Net;
using FluentAssertions;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

public class AuctionTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public AuctionTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    // --------------------------------------------------GET REQUESTS--------------------------------------------------
    // GET /auctions test
    [Fact(DisplayName = "[GET /auctions returns 200 OK]")]
    public async Task GetAuctions()
    {
        var response = await _client.GetAsync("/auctions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "[GET /auctions with specific ID & returns 200 OK]")]
    public async Task GetAuctionsWithID()
    {
        // Create an auction else we can't get it
        var body = new
        {
            auctionneerId = Guid.NewGuid(),
            startTime = DateTime.Parse("2025-12-01T09:00:00Z"),
            endTime = DateTime.Parse("2025-12-01T10:00:00Z"),
            status = "Planned"
        };

        var createResponse = await _client.PostAsJsonAsync("/auctions", body);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Read returned JSON from the POST to get the auction ID
        var createdAuction = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = createdAuction.GetProperty("id").GetGuid();

        // GET /auctions/{id}
        var getResponse = await _client.GetAsync($"/auctions/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Deserialize into C# object
        var auction = await getResponse.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        auction.GetProperty("status").GetString().Should().Be("Planned");
    }

    // --------------------------------------------------POST REQUESTS--------------------------------------------------
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

        // Send POST request
        var response = await _client.PostAsJsonAsync("/auctions", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
