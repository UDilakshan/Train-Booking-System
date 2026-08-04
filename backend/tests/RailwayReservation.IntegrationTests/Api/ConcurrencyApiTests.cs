using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RailwayReservation.Domain.Enums;
using RailwayReservation.IntegrationTests.Infrastructure;
using Xunit;

namespace RailwayReservation.IntegrationTests.Api;

/// <summary>
/// The flagship proof of the concurrency strategy (see README "Concurrency Strategy" and
/// BookingRepository.CreateAsync): fire many overlapping booking requests for the same seat at
/// once and assert that the row lock + the UNIQUE(seat_id, journey_id, leg_order) constraint let
/// exactly one win, with no double booking, regardless of request interleaving.
/// </summary>
public class ConcurrencyApiTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private TestFixtureData _fixture = null!;

    public ConcurrencyApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
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
    public async Task Allows_exactly_one_of_n_simultaneous_requests_for_the_identical_segment_on_one_seat()
    {
        const int concurrentRequests = 15;
        var seatId = _fixture.SeatIds[0];

        var tasks = Enumerable.Range(0, concurrentRequests).Select(i =>
        {
            var client = _factory.CreateClient();
            return client.PostAsJsonAsync("/bookings", new
            {
                journeyId = _fixture.JourneyId,
                originStationId = _fixture.StationA,
                destinationStationId = _fixture.StationC,
                passengerName = $"Concurrent Passenger {i}",
                passengerContact = $"07700000{i:D2}",
                seatIds = new[] { seatId },
            });
        });

        var results = await Task.WhenAll(tasks);

        results.Count(r => r.StatusCode == HttpStatusCode.Created).Should().Be(1);
        results.Count(r => r.StatusCode == HttpStatusCode.Conflict).Should().Be(concurrentRequests - 1);

        foreach (var conflict in results.Where(r => r.StatusCode == HttpStatusCode.Conflict))
        {
            (await conflict.ErrorAsync()).Code.Should().Be("SEGMENT_OVERLAP");
        }

        await using var db = _factory.CreateDbContext();
        var confirmedSegments = await db.BookingSegments.CountAsync(s => s.SeatId == seatId && s.JourneyId == _fixture.JourneyId && s.Status == BookingStatus.Confirmed);
        confirmedSegments.Should().Be(1);
    }

    [Fact]
    public async Task Allows_concurrent_requests_for_genuinely_adjacent_segments_on_the_same_seat_to_all_succeed()
    {
        var seatId = _fixture.SeatIds[1];

        var segments = new (Guid Origin, Guid Destination, string Name)[]
        {
            (_fixture.StationA, _fixture.StationB, "Passenger AB"),
            (_fixture.StationB, _fixture.StationC, "Passenger BC"),
            (_fixture.StationC, _fixture.StationD, "Passenger CD"),
        };

        var tasks = segments.Select(s =>
        {
            var client = _factory.CreateClient();
            return client.PostAsJsonAsync("/bookings", new
            {
                journeyId = _fixture.JourneyId,
                originStationId = s.Origin,
                destinationStationId = s.Destination,
                passengerName = s.Name,
                passengerContact = "0770000000",
                seatIds = new[] { seatId },
            });
        });

        var results = await Task.WhenAll(tasks);

        results.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.Created);

        await using var db = _factory.CreateDbContext();
        var confirmedSegments = await db.BookingSegments.CountAsync(s => s.SeatId == seatId && s.JourneyId == _fixture.JourneyId && s.Status == BookingStatus.Confirmed);
        confirmedSegments.Should().Be(3);
    }
}
