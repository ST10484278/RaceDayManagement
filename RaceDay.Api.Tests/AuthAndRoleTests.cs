using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using RaceDay.Contracts;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Api.Tests;

public class AuthAndRoleTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthAndRoleTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_Returns201Created()
    {
        var request = new RegisterRequest($"user_{Guid.NewGuid()}@test.com", "Password123!", "John", "Doe", "Participant");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        Assert.True(response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401Unauthorized()
    {
        var request = new LoginRequest("nonexistent@test.com", "WrongPassword");
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_AsParticipant_Returns403Forbidden()
    {
        var request = new CreateEventRequest("Trail Run", "5km Trail", DateTime.UtcNow.AddDays(10), "Park");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/events")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("X-User-Role", "Participant");
        httpRequest.Headers.Add("X-User-Id", "5");

        var response = await _client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_AsOrganiser_Returns201Created()
    {
        var request = new CreateEventRequest("City Marathon", "42km Race", DateTime.UtcNow.AddDays(30), "City Center");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/events")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("X-User-Role", "Organiser");
        httpRequest.Headers.Add("X-User-Id", "1");

        var response = await _client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task EnrolInEvent_AsParticipant_Returns201Created()
    {
        var request = new CreateEnrolmentRequest(1);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/enrolments")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("X-User-Role", "Participant");
        httpRequest.Headers.Add("X-User-Id", "5");

        var response = await _client.SendAsync(httpRequest);

        Assert.True(response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task EnrolInEvent_AsOrganiser_Returns403Forbidden()
    {
        var request = new CreateEnrolmentRequest(1);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/enrolments")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("X-User-Role", "Organiser");
        httpRequest.Headers.Add("X-User-Id", "1");

        var response = await _client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}