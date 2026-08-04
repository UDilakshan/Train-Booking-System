using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using RailwayReservation.Domain.Entities;
using RailwayReservation.Domain.Enums;
using RailwayReservation.IntegrationTests.Infrastructure;
using Xunit;

namespace RailwayReservation.IntegrationTests.Api;

public class AuthApiTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private const string Email = "e2e-admin@railway.lk";
    private const string Password = "E2eTestPassword123!";

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await using var db = _factory.CreateDbContext();
        var existing = db.Users.FirstOrDefault(u => u.Email == Email);
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(Password, workFactor: 4); // low cost factor: speed, not security, in tests
        if (existing is null)
        {
            db.Users.Add(new User { Id = Guid.NewGuid(), Email = Email, Name = "E2E Admin", Role = UserRole.Admin, PasswordHash = passwordHash, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        }
        else
        {
            existing.PasswordHash = passwordHash;
        }
        await db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Rejects_admin_endpoints_without_a_token()
    {
        var res = await _client.GetAsync("/admin/bookings");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Rejects_login_with_an_incorrect_password()
    {
        var res = await _client.PostAsJsonAsync("/auth/login", new { email = Email, password = "wrong-password" });
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await res.ErrorAsync()).Code.Should().Be("INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Logs_in_with_correct_credentials_and_can_then_access_an_admin_endpoint()
    {
        var loginRes = await _client.PostAsJsonAsync("/auth/login", new { email = Email, password = Password });
        loginRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await loginRes.DataAsync();
        var token = data.GetProperty("accessToken").GetString();
        token.Should().NotBeNullOrEmpty();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/admin/bookings");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var adminRes = await _client.SendAsync(request);
        adminRes.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
