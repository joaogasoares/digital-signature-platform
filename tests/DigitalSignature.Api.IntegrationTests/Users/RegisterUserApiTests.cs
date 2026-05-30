using System.Net;
using System.Net.Http.Json;
using DigitalSignature.Api.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace DigitalSignature.Api.IntegrationTests.Users;

public sealed class RegisterUserApiTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_valid_user_returns_201()
    {
        var response = await _client.PostAsJsonAsync("/api/users/register", new
        {
            email = $"test-{Guid.NewGuid()}@example.com",
            password = "StrongP4ss!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_duplicate_email_returns_400()
    {
        var email = $"dup-{Guid.NewGuid()}@example.com";

        await _client.PostAsJsonAsync("/api/users/register", new
        {
            email,
            password = "StrongP4ss!"
        });

        var response = await _client.PostAsJsonAsync("/api/users/register", new
        {
            email,
            password = "StrongP4ss!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_valid_credentials_returns_token()
    {
        var email = $"login-{Guid.NewGuid()}@example.com";

        await _client.PostAsJsonAsync("/api/users/register", new
        {
            email,
            password = "StrongP4ss!"
        });

        var response = await _client.PostAsJsonAsync("/api/users/login", new
        {
            email,
            password = "StrongP4ss!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        body.Should().ContainKey("token");
        body!["token"].Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_wrong_password_returns_401()
    {
        var response = await _client.PostAsJsonAsync("/api/users/login", new
        {
            email = "nobody@example.com",
            password = "WrongPass1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
