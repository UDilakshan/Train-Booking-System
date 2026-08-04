using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RailwayReservation.IntegrationTests.Infrastructure;
using Xunit;

namespace RailwayReservation.IntegrationTests.Api;

public class AvailabilityApiTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private TestFixtureData _fixture = null!;

    public AvailabilityApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await using var db = _factory.CreateDbContext();
        _fixture = await TestFixture.SeedAsync(db);
    }

    public async Task DisposeAsync()
    {
        await using var db = _factory.CreateDbContext();
        await TestFixture.ResetBookingsAsync(db, _fixture);
    }

    [Fact]
    public async Task Reports_every_seat_available_when_nothing_has_been_booked()
    {
        var res = await _client.GetAsync($"/availability?journeyId={_fixture.JourneyId}&originStationId={_fixture.StationA}&destinationStationId={_fixture.StationB}");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await res.DataAsync();
        data.GetArrayLength().Should().Be(_fixture.SeatIds.Count);
        data.EnumerateArray().Should().OnlyContain(s => s.GetProperty("isAvailable").GetBoolean());
    }

    [Fact]
    public async Task Marks_a_seat_unavailable_only_for_segments_that_overlap_an_existing_booking()
    {
        var seatId = _fixture.SeatIds[0];

        var createRes = await _client.PostAsJsonAsync("/bookings", new
        {
            journeyId = _fixture.JourneyId,
            originStationId = _fixture.StationB,
            destinationStationId = _fixture.StationC,
            passengerName = "Nimal Perera",
            passengerContact = "0771234567",
            seatIds = new[] { seatId },
        });
        createRes.StatusCode.Should().Be(HttpStatusCode.Created);

        var overlapping = await (await _client.GetAsync($"/availability?journeyId={_fixture.JourneyId}&originStationId={_fixture.StationA}&destinationStationId={_fixture.StationC}")).DataAsync();
        var bookedSeat = overlapping.EnumerateArray().First(s => s.GetProperty("seatId").GetGuid() == seatId);
        bookedSeat.GetProperty("isAvailable").GetBoolean().Should().BeFalse();

        var adjacentBefore = await (await _client.GetAsync($"/availability?journeyId={_fixture.JourneyId}&originStationId={_fixture.StationA}&destinationStationId={_fixture.StationB}")).DataAsync();
        adjacentBefore.EnumerateArray().First(s => s.GetProperty("seatId").GetGuid() == seatId).GetProperty("isAvailable").GetBoolean().Should().BeTrue();

        var adjacentAfter = await (await _client.GetAsync($"/availability?journeyId={_fixture.JourneyId}&originStationId={_fixture.StationC}&destinationStationId={_fixture.StationD}")).DataAsync();
        adjacentAfter.EnumerateArray().First(s => s.GetProperty("seatId").GetGuid() == seatId).GetProperty("isAvailable").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task Rejects_a_request_where_origin_is_not_before_destination()
    {
        var res = await _client.GetAsync($"/availability?journeyId={_fixture.JourneyId}&originStationId={_fixture.StationC}&destinationStationId={_fixture.StationA}");
        res.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var (code, _) = await res.ErrorAsync();
        code.Should().Be("INVALID_SEGMENT");
    }
}
