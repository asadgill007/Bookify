using System.Net;
using FluentAssertions;

namespace Bookify.WebApi.Tests;

/// <summary>
/// Smoke tests for the WebApi project.
/// These tests verify the application starts and basic endpoints respond.
/// Requires the API to be running (for true integration tests, would use WebApplicationFactory).
/// </summary>
public class HealthEndpointSmokeTests
{
    private readonly HttpClient _client;

    public HealthEndpointSmokeTests()
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
    }

    [Fact(Skip = "Requires running API. Start with: dotnet run --project src/Bookify.WebApi")]
    public async Task HealthEndpoint_Returns200()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
    }

    [Fact(Skip = "Requires running API")]
    public async Task HealthReadinessEndpoint_Returns200()
    {
        var response = await _client.GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(Skip = "Requires running API")]
    public async Task SwaggerEndpoint_Returns200()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("openapi");
    }

    [Fact(Skip = "Requires running API with seeded data")]
    public async Task CategoriesEndpoint_Returns200()
    {
        var response = await _client.GetAsync("/api/v1/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
