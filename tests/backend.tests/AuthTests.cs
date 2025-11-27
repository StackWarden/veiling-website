namespace backend.tests;

using System.Net;
using FluentAssertions;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

public class AuthTests : IClassFixture<TestFactory>
{
    private readonly HttpClient _client;

    public AuthTests(TestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "Register, Login and Access Protected Endpoint")]
    public async Task AuthFlow()
    {
        // 1. Register
        var registerDto = new
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "Password123!"
        };

        var registerResponse = await _client.PostAsJsonAsync("/auth/register", registerDto);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Login
        var loginDto = new
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/auth/jwt", loginDto);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Extract JWT from Set-Cookie header
        var cookies = loginResponse.Headers.GetValues("Set-Cookie");
        var jwtCookie = cookies.FirstOrDefault(c => c.StartsWith("jwt="));
        jwtCookie.Should().NotBeNull();

        // 3. Access Protected Endpoint (/auth/info)
        // Add the cookie to the request
        var request = new HttpRequestMessage(HttpMethod.Get, "/auth/info");
        request.Headers.Add("Cookie", jwtCookie);

        var infoResponse = await _client.SendAsync(request);
        infoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var info = await infoResponse.Content.ReadFromJsonAsync<JsonElement>();
        info.GetProperty("name").GetString().Should().Be("Test User");
    }
}
