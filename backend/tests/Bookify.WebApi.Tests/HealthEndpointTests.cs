using System.Net;
using System.Net.Http.Json;
using Bookify.WebApi.Tests.IntegrationTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Bookify.WebApi.Tests;

/// <summary>
/// Smoke tests for the WebApi project using the in-process test factory,
/// so no live server is required.
/// </summary>
public class HealthEndpointSmokeTests : IClassFixture<BookifyTestApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointSmokeTests(BookifyTestApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost:5001"),
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task HealthEndpoint_Returns200()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.ToLowerInvariant().Should().Contain("healthy");
    }

    [Fact]
    public async Task SwaggerEndpoint_Returns200()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Redirect,
            HttpStatusCode.RedirectKeepVerb);
    }

    [Fact]
    public async Task CategoriesEndpoint_Returns200()
    {
        var response = await _client.GetAsync("/api/v1/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task BusinessSearch_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/v1/businesses?page=1&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
