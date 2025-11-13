namespace backend.tests;

using System.Net;
using FluentAssertions;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

public class AuctionTests : IClassFixture<TestFactory>
{
    private readonly HttpClient _client;

    public AuctionTests(TestFactory factory)
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

    // GET /auctions/{id} test
    [Fact(DisplayName = "[GET /auctions with specific ID & returns 200 OK]")]
    public async Task GetAuctionsById()
    {
        // Create an auction else we can't get it
        var body = new
        {
            auctionneerId = Guid.NewGuid(),
            startTime = DateTime.Parse("2025-12-01T09:00:00Z"),
            endTime = DateTime.Parse("2025-12-01T10:00:00Z"),
            status = "Planned"
        };

        // POST /auctions
        var createResponse = await _client.PostAsJsonAsync("/auctions", body);
        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Read returned JSON from the POST to get the auction ID
        var createdAuction = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        // Get the ID
        var id = createdAuction.GetProperty("id").GetGuid();

        // GET /auctions/{id}
        var getResponse = await _client.GetAsync($"/auctions/{id}");
        // Assert
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

    // --------------------------------------------------PUT REQUESTS--------------------------------------------------
    // PUT /auctions/{id} test
    [Fact(DisplayName = "[PUT /auctions/{id} updates status to Finished & returns 200 OK]")]
    public async Task PutAuctionsById()
    {
        // Create an auction else we can't get it
        var body = new
        {
            auctionneerId = Guid.NewGuid(),
            startTime = DateTime.Parse("2025-12-01T09:00:00Z"),
            endTime = DateTime.Parse("2025-12-01T10:00:00Z"),
            status = "Planned"
        };

        // Create auction
        var createResponse = await _client.PostAsJsonAsync("/auctions", body);
        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Read returned JSON from the POST to get the auction ID
        var createdAuction = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        // Get the ID
        var id = createdAuction.GetProperty("id").GetGuid();

        // Update body
        var updatedBody = new
        {
            body.auctionneerId,
            body.startTime,
            body.endTime,
            status = "Finished"
        };

        // Send PUT request
        var putResponse = await _client.PutAsJsonAsync($"/auctions/{id}", updatedBody);
        // Assert
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify update by getting the auction again
        var getResponse = await _client.GetAsync($"/auctions/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Deserialize into C# object
        var updatedAuction = await getResponse.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        updatedAuction.GetProperty("status").GetString().Should().Be("Finished");
    }

    // --------------------------------------------------DELETE REQUESTS--------------------------------------------------
    // DELETE /auctions/{id} test
    [Fact(DisplayName = "[DELETE /auctions/{id} deletes the auction & returns 200 OK]")]
    public async Task DeleteAuctionsById()
    {
        // Create auction to delete
        var body = new
        {
            auctionneerId = Guid.NewGuid(),
            startTime = DateTime.Parse("2025-12-01T09:00:00Z"),
            endTime = DateTime.Parse("2025-12-01T10:00:00Z"),
            status = "Planned"
        };

        // POST /auctions
        var createResponse = await _client.PostAsJsonAsync("/auctions", body);
        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Extract ID
        var createdAuction = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = createdAuction.GetProperty("id").GetGuid();

        // DELETE /auctions/{id}
        var deleteResponse = await _client.DeleteAsync($"/auctions/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // GET should be 404 Not Found
        var getResponse = await _client.GetAsync($"/auctions/{id}");
        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
